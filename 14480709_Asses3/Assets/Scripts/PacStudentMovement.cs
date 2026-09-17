using UnityEngine;
using System.Collections;


public class PacStudentMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource movementAudio;

    private readonly Vector3[] waypoints =
    {
        new Vector3(1f, -1f, 0f),
        new Vector3(6f, -1f, 0f),
        new Vector3(6f, -5f, 0f),
        new Vector3(1f, -5f, 0f)
    };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        transform.position = waypoints[0];

        if (movementAudio != null)
        {
            movementAudio.Play();
        }

        StartCoroutine(MoveAroundBlock());
    }

    private IEnumerator MoveAroundBlock()
    {
        int currentWaypoint = 0;

        while (true)
        {
            int nextWaypoint = (currentWaypoint + 1) % waypoints.Length;

            Vector3 startPosition = waypoints[currentWaypoint];
            Vector3 endPosition = waypoints[nextWaypoint];

            SetMovementAnimation(startPosition, endPosition);

            float distance = Vector3.Distance(startPosition, endPosition);
            float duration = distance / moveSpeed;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                float t = Mathf.Clamp01(elapsedTime / duration);

                transform.position = Vector3.Lerp(startPosition, endPosition, t);

                yield return null;
            }

            transform.position = endPosition;

            currentWaypoint = nextWaypoint;
        }
    }

    private void SetMovementAnimation(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 direction = endPosition - startPosition;

        if (direction.x > 0f)
        {
            animator.Play("PacStudent_Right");
        }
        else if (direction.x < 0f)
        {
            animator.Play("PacStudent_Left");
        }
        else if (direction.y > 0f)
        {
            animator.Play("PacStudent_Up");
        }
        else if (direction.y < 0f)
        {
            animator.Play("PacStudent_Down");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
