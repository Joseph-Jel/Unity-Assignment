using UnityEngine;

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
    void Update()
    {
        
    }
}
