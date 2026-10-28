using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PacStudentController : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioManager gameAudio;
    [SerializeField] private Collider2D interactionCollider;

    [Header("Collision Effects")]
    [SerializeField] private ParticleSystem wallBumpParticlePrefab;
    [SerializeField] private ParticleSystem deathParticlePrefab;

    [Header("Death")]
    [SerializeField] private string deathAnimationState = "PacStudent_Dead";

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
    private bool controlEnabled = false;
    private bool wallBumpLocked = false;
    private bool teleportLocked = false;

    private Vector2Int currentGridPosition;

    private Vector3 lerpStartPosition;
    private Vector3 lerpTargetPosition;

    private float lerpElapsedTime;
    private float lerpDuration;

    private static readonly Vector2Int GridUp = new Vector2Int(0, -1);
    private static readonly Vector2Int GridDown = new Vector2Int(0, 1);
    private static readonly Vector2Int GridLeft = new Vector2Int(-1, 0);
    private static readonly Vector2Int GridRight = new Vector2Int(1, 0);

    private Vector2Int startingGridPosition;


    public float MoveSpeed
    {
        get
        {
            return moveSpeed;
        }
    }    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentGridPosition = levelGenerator.WorldToGrid(transform.position);

        PlayDirectionAnimation(GridRight);

        if (animator != null)
        {
            animator.speed = 0f;
        }

        if (dustParticles != null)
        {
            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        startingGridPosition = currentGridPosition;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!controlEnabled)
        {
            return;
        }

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
            wallBumpLocked = false;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            lastInput = GridLeft;
            wallBumpLocked = false;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            lastInput = GridDown;
            wallBumpLocked = false;
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            lastInput = GridRight;
            wallBumpLocked = false;
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

        Vector2Int bumpDirection = currentInput != Vector2Int.zero ? currentInput : lastInput;

        if(!wallBumpLocked && bumpDirection != Vector2Int.zero)
        {
            PlayWallBump(bumpDirection);

            wallBumpLocked = true;
        }

        StopMovementEffects();
    }

    private void PlayWallBump(Vector2Int direction)
    {
        if  (gameAudio != null)
        {
            gameAudio.PlayWallCollision();
        }

        if (wallBumpParticlePrefab == null)
        {
            return;
        }

        Vector3 worldDirection = new Vector3(direction.x, -direction.y, 0f);

        Vector3 collisionPoint = transform.position + worldDirection * (levelGenerator.TileSize * 0.5f);

        ParticleSystem effect = Instantiate(wallBumpParticlePrefab, collisionPoint, Quaternion.identity);

        effect.Play();

        ParticleSystem.MainModule main = effect.main;

        Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax);
    }

    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;

        if (!enabled)
        {
            isLerping = false;

            StopMovementEffects();
        }
    }

    public void SetCollisionEnabled(bool enabled)
    {
        if (interactionCollider != null)
        {
            interactionCollider.enabled = enabled;
        }
    }

    public void ResetToStart()
    {
        isLerping = false;

        currentGridPosition = startingGridPosition;

        transform.position = levelGenerator.GridToWorld(startingGridPosition);

        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;


        wallBumpLocked = false;

        PlayDirectionAnimation(GridRight);

        if (animator != null)
        {
            animator.speed = 0f;
        }

        StopMovementEffects();
    }

    public void PlayDeath()
    {
        isLerping = false;

        StopMovementEffects();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play(deathAnimationState);
        }

        if (deathParticlePrefab != null)
        {
            ParticleSystem effect = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

            effect.Play();

            ParticleSystem.MainModule main = effect.main;

            Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax);
        }    
    }

    public void TeleportTo(Vector2Int destinationGrid, Vector2Int inwardDirection)
    {
        if (teleportLocked)
        {
            return;
        }

        teleportLocked = true;

        isLerping = false;

        currentGridPosition = destinationGrid;

        transform.position = levelGenerator.GridToWorld(destinationGrid);

        currentInput = inwardDirection;

        lastInput = inwardDirection;

        StartCoroutine(UnlockTeleporter());
    }

    private System.Collections.IEnumerator UnlockTeleporter()
    {
        yield return new WaitForSeconds(0.15f);

        teleportLocked = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!controlEnabled)
        {
            return;
        }

        Teleporter teleporter = other.GetComponent<Teleporter>();

        if (teleporter != null)
        {
            TeleportTo(teleporter.DestinationGrid, teleporter.InwardDirection);

            return;
        }

        if (other.CompareTag("Pellet"))
        {
            Vector2Int pelletGrid = levelGenerator.WorldToGrid(other.transform.position);

            levelGenerator.MarkTileEmpty(pelletGrid);

            Destroy(other.gameObject);

            gameManager.CollectPellet();

            return;
        }

        if (other.CompareTag("PowerPellet"))
        {
            Vector2Int pelletGrid = levelGenerator.WorldToGrid(other.transform.position);

            levelGenerator.MarkTileEmpty(pelletGrid);

            Destroy(other.gameObject);

            gameManager.CollectPowerPellet();

            return;
        }

        if (other.CompareTag("Cherry"))
        {
            Destroy(other.gameObject);

            gameManager.CollectCherry();

            return;
        }

        if (other.CompareTag("Ghost"))
        {
            GhostStateController ghost = other.GetComponent<GhostStateController>();

            if (ghost == null)
            {
                ghost = other.GetComponentInParent<GhostStateController>();
            }

            if (ghost != null)
            {
                gameManager.HandleGhostCollision(ghost);
            }
        }
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
