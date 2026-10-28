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


    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GhostController ghostController;
    [SerializeField] private Collider2D interactionCollider;

    [Header("Animator State Names")]
    [SerializeField] private string normalState = "Normal";
    [SerializeField] private string scaredState = "Scared";
    [SerializeField] private string recoveringState = "Recovering";
    [SerializeField] private string deadState = "Dead";

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

        if(interactionCollider != null)
        {
            interactionCollider.enabled = newState != GhostState.Dead;
        }

        if (animator == null)
        {
            return;
        }

        switch (newState)
        {
            case GhostState.Normal:
                animator.Play(normalState, 0);
                break;

            case GhostState.Scared:
                animator.Play(scaredState, 0);
                break;

            case GhostState.Recovering:
                animator.Play(recoveringState, 0);
                break;

            case GhostState.Dead:
                animator.Play(deadState, 0);
                break;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (ghostController != null)
        {
            ghostController.enabled = enabled;
        }
    }

    public void ResetGhost()
    {
        transform.position = startingPosition;

        SetState(GhostState.Normal);

        if (ghostController != null)
        {
            ghostController.ResetMovementFromCurrentPosition();
        }
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
