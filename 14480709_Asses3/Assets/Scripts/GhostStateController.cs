using UnityEngine;

public enum GhostState
{
    Normal,
    Scared,
    Recovering,
    Dead
}

public class GhostStateController : MonoBehaviour
{

    [SerializeField] private Animator animator;

    [Header("Animator State Names")]
    [SerializeField] private string normalState = "Normal";
    [SerializeField] private string scaredState = "Scared";
    [SerializeField] private string recoveringState = "Recovering";
    [SerializeField] private string deadState = "Dead";

    [Header("Movement Script")]
    [SerializeField] private MonoBehaviour movementController;

    private Vector3 startingPosition;

    public GhostState CurrentState { get; private set; }

    private void Awake()
    {
        startingPosition = transform.position;
        CurrentState = GhostState.Normal;
    }

    public void SetState(GhostState newState)
    {
        CurrentState = newState;

        if (animator == null)
        {
            return;
        }

        switch (newState)
        {
            case GhostState.Normal:
                animator.Play(normalState);
                break;

            case GhostState.Scared:
                animator.Play(scaredState);
                break;

            case GhostState.Recovering:
                animator.Play(recoveringState);
                break;

            case GhostState.Dead:
                animator.Play(deadState);
                break;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (movementController != null)
        {
            movementController.enabled = enabled;
        }
    }

    public void ResetGhost()
    {
        transform.position = startingPosition;

        SetState(GhostState.Normal);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
