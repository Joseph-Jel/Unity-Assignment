using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioSource movementAudioSource;
    [SerializeField] private AudioClip movingAudio;
    [SerializeField] private AudioClip eatingPelletAudio;

    [Header("Particles")]
    [SerializeField] private ParticleSystem dustParticles;

    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;

    private bool isLerping = false;

    private Vector2Int currentGridPosition;

    private Vector3 lerpStartPosition;
    private Vector3 lerpTargetPosition;

    private float lerpElapsedTime;
    private float lerpDuration;

    private static readonly Vector2Int GridUp = new Vector2Int(0, -1);
    private static readonly Vector2Int GridDown = new Vector2Int(0, 1);
    private static readonly Vector2Int GridLeft = new Vector2Int(-1, 0);
    private static readonly Vector2Int GridRight = new Vector2Int(1, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentGridPosition = levelGenerator.WorldToGrid(transform.position);

        PlayDirectionAnimation(Vector2Int.right);

        if (animator != null)
        {
            animator.speed = 0f;
        }

        if (dustParticles != null)
        {
            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        GatherInput();

        if (levelGenerator == null || !levelGenerator.IsReady)
        {
            return;
        }

        if (isLerping)
        {
            UpdateLerp();
        }

        if (!isLerping)
        {
            TryStartNextMove();
        }
    }

    private void GatherInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            lastInput = GridUp;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            lastInput = GridLeft;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            lastInput = GridDown;
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            lastInput = GridRight;
        }
    }

    private void TryStartNextMove()
    {
        if (lastInput != Vector2Int.zero && CanMove(lastInput))
        {
            currentInput = lastInput;
            StartMove(currentInput);
            return;
        }

        if (currentInput != Vector2Int.zero && CanMove(currentInput))
        {
            StartMove(currentInput);
            return;
        }

        StopMovementEffects();
    }

    private bool CanMove(Vector2Int direction)
    {
        Vector2Int nextGridPosition = currentGridPosition + direction;

        return levelGenerator.IsWalkable(nextGridPosition);
    }

    private void StartMove(Vector2Int direction)
    {
        Vector2Int targetGridPosition = currentGridPosition + direction;

        lerpStartPosition = levelGenerator.GridToWorld(currentGridPosition); 

        lerpTargetPosition = levelGenerator.GridToWorld(targetGridPosition);

        float distance = Vector3.Distance(lerpStartPosition, lerpTargetPosition);

        lerpDuration = distance / moveSpeed;
        lerpElapsedTime = 0f;

        isLerping = true;

        PlayDirectionAnimation(direction);
        StartMovementEffects(targetGridPosition);
    }

    private void UpdateLerp()
    {
        lerpElapsedTime += Time.deltaTime;

        float t = Mathf.Clamp01(lerpElapsedTime / lerpDuration);

        transform.position = Vector3.Lerp(lerpStartPosition, lerpTargetPosition, t);

        if (t >= 1f)
        {
            transform.position = lerpTargetPosition;

            currentGridPosition = levelGenerator.WorldToGrid(lerpTargetPosition);

            isLerping = false;
        }
    }

    private void PlayDirectionAnimation(Vector2Int direction)
    {
        if (animator == null)
        {
            return;
        }

        animator.speed = 1f;

        if (direction == GridUp)
        {
            animator.Play("PacStudent_Up");
        }
        else if (direction == GridDown)
        {
            animator.Play("PacStudent_Down");
        }
        else if (direction == GridLeft)
        {
            animator.Play("PacStudent_Left");
        }
        else if (direction == GridRight)
        {
            animator.Play("PacStudent_Right");
        }
    }

    private void StartMovementEffects(Vector2Int targetGridPosition)
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }

        if (dustParticles != null && !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }

        PlayMovementAudio(targetGridPosition);
    }

    private void StopMovementEffects()
    {
        if (animator != null)
        {
            animator.speed = 0f;
        }

        if (movementAudioSource != null && movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }

        if (dustParticles != null && dustParticles.isPlaying)
        {
            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void PlayMovementAudio(Vector2Int targetGridPosition)
    {
        if (movementAudioSource == null)
        {
            return;
        }

        int targetTile = levelGenerator.GetTileAtGrid(targetGridPosition);

        AudioClip desiredClip;

        if (targetTile == 5 || targetTile == 6)
        {
            desiredClip = eatingPelletAudio;
        }
        else
        {
            desiredClip = movingAudio;
        }

        if (desiredClip == null)
        {
            return;
        }

        if (movementAudioSource.clip != desiredClip || !movementAudioSource.isPlaying)
        {
            movementAudioSource.clip = desiredClip;
            movementAudioSource.loop = true;
            movementAudioSource.Play();
        }
    }
}
