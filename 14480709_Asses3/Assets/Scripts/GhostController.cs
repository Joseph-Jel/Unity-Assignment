using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Tilemaps;

public class GhostController : MonoBehaviour
{
    [Header("Ghost")]
    [SerializeField, Range(1, 4)]
    private int ghostNumber = 1;

    [Header("References")]
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private PacStudentController pacStudentController;
    [SerializeField] private Transform pacStudent;
    [SerializeField] private GhostStateController stateController;
    [SerializeField] private GhostSpawnArea spawnArea;
    [SerializeField] private GameManager gameManager;

    [Header("Teleporters")]
    [SerializeField] private Teleporter[] teleporters;

    [Header("Speed Multipliers")]
    [SerializeField] private float normalMultiplier = 0.9f;
    [SerializeField] private float scaredMultiplier = 0.5f;

    private static readonly Vector2Int GridUp = new Vector2Int(0, -1);
    private static readonly Vector2Int GridDown = new Vector2Int(0, 1);
    private static readonly Vector2Int GridLeft = new Vector2Int(-1, 0);
    private static readonly Vector2Int GridRight = new Vector2Int(1, 0);

    private static readonly Vector2Int[] Directions =
    {
        GridUp, GridDown, GridLeft, GridRight
    };

    private Vector2Int currentGridPosition;
    private Vector2Int currentDirection = Vector2Int.zero;

    private Vector3 startingWorldPosition;
    private Vector3 lerpStartPosition;
    private Vector3 lerpTargetPosition;

    private float lerpProgress;
    private float lerpDistance;

    private bool isLerping;
    private bool movementEnabled;
    private bool hasExitedSpawnArea;

    private GhostState previousState;

    private Vector3 deadStartPosition;
    private Vector3 deadTargetPosition;

    private float deadProgress;
    private float deadDistance;

    private bool deadArrivalHandled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        startingWorldPosition = transform.position;

        currentGridPosition = levelGenerator.WorldToGrid(transform.position);

        hasExitedSpawnArea = !spawnArea.ContainsWorldPosition(transform.position);

        previousState = stateController.CurrentState;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!movementEnabled || levelGenerator == null || !levelGenerator.IsReady)
        {
            return;
        }

        GhostState state = stateController.CurrentState;

        if (state != previousState)
        {
            if (state == GhostState.Dead)
            {
                BeginDeadMovement();
            }

            previousState = state;
        }

        if (state == GhostState.Dead)
        {
            UpdateDeadMovement();

            return;
        }

        if (isLerping)
        {
            UpdateGridLerp();
        }

        if(!isLerping)
        {

            ChooseAndStartNextMove();
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            isLerping = false;
        }
    }

    public void ResetMovementFromCurrentPosition()
    {
        currentGridPosition = levelGenerator.WorldToGrid(transform.position);

        currentDirection = Vector2Int.zero;

        isLerping = false;

        lerpProgress = 0f;

        hasExitedSpawnArea = false;

        deadArrivalHandled = false;

        previousState = stateController.CurrentState;
    }

    private void ChooseAndStartNextMove()
    {
        Vector2Int direction = Vector2Int.zero;

        if (!hasExitedSpawnArea)
        {
            Vector2Int exitGrid = spawnArea.GetExitGrid(ghostNumber, levelGenerator);

            if (currentGridPosition == exitGrid)
            {
                hasExitedSpawnArea = true;

                Debug.Log($"Ghost {ghostNumber} successfully exited spawn!");
            }
            else
            {
                direction = ChooseSpawnExitDirection();

                if (direction == Vector2Int.zero)
                {
                    Debug.LogError($"Ghost {ghostNumber} cannot exit." + $"Current grid: {currentGridPosition}, " + $"Target: {exitGrid}");

                    return;
                }

                StartGridMove(direction);
                return;
            }
        }

        switch (stateController.CurrentState)
        {
            case GhostState.Scared:
            case GhostState.Recovering:
                direction = ChooseAwayFromPacStudent();
                break;

            case GhostState.Normal:
                direction = ChooseNormalDirection();
                break;
        }

        if (direction != Vector2Int.zero)
        {
            StartGridMove(direction);
        }
        else
        {
            Debug.LogWarning($"Ghost {ghostNumber} has no valid movement direction " + $"at {currentGridPosition}");
        }
    }

    private Vector2Int ChooseNormalDirection()
    {
        switch (ghostNumber)
        {
            case 1:
                return ChooseAwayFromPacStudent();

            case 2:
                return ChooseTowardPacStudent();

            case 3:
                return ChooseRandomDirection();

            case 4:
                return ChooseClockwiseOuterDirection();

            default:
                return ChooseRandomDirection();
        }
    }

    private Vector2Int ChooseAwayFromPacStudent()
    {
        List<Vector2Int> valid = GetDirectionsWithoutBackstep(GetValidDirections(false));

        if (valid.Count == 0)
        {
            return Vector2Int.zero;
        }

        float currentDistance = Vector2.Distance(transform.position, pacStudent.position);

        List<Vector2Int> preferred = new List<Vector2Int>();

        foreach (Vector2Int direction in valid)
        {
            Vector2Int nextGrid = currentGridPosition + direction;

            Vector3 nextWorld = levelGenerator.GridToWorld(nextGrid);

            float newDistance = Vector2.Distance(nextWorld, pacStudent.position);

            if (newDistance >= currentDistance - 0.001f)
            {
                preferred.Add(direction);
            }
        }

        if (preferred.Count > 0)
        {
            return preferred[Random.Range(0, preferred.Count)];
        }

        return valid[Random.Range(0, valid.Count)];
    }

    private Vector2Int ChooseTowardPacStudent()
    {
        List<Vector2Int> valid = GetDirectionsWithoutBackstep(GetValidDirections(false));

        if (valid.Count == 0)
        {
            return Vector2Int.zero;
        }

        float currentDistance = Vector2.Distance(transform.position, pacStudent.position);

        List<Vector2Int> preferred = new List<Vector2Int>();

        foreach (Vector2Int direction in valid)
        {
            Vector2Int nextGrid = currentGridPosition + direction;

            Vector3 nextWorld = levelGenerator.GridToWorld(nextGrid);

            float newDistance = Vector2.Distance(nextWorld, pacStudent.position);

            if (newDistance <= currentDistance + 0.001f)
            {
                preferred.Add(direction);
            }
        }

        if (preferred.Count > 0 )
        {
            return preferred[Random.Range(0, preferred.Count)];
        }

        return valid[Random.Range(0, valid.Count)];
    }

    private Vector2Int ChooseRandomDirection()
    {
        List<Vector2Int> valid = GetDirectionsWithoutBackstep(GetValidDirections(false));

        if (valid.Count == 0)
        {
            return Vector2Int.zero;
        }

        return valid[Random.Range(0, valid.Count)];
    }

    private Vector2Int ChooseClockwiseOuterDirection()
    {
        List<Vector2Int> valid = GetDirectionsWithoutBackstep(GetValidDirections(false));

        if (valid.Count == 0)
        {
            return Vector2Int.zero;
        }

        int smallestEdgeDistance = int.MaxValue;

        List<Vector2Int> outerCandidates = new List<Vector2Int>();

        foreach (Vector2Int direction in valid)
        {
            Vector2Int next = currentGridPosition + direction;

            int edgeDistance = GetDistanceFromMapEdge(next);

            if (edgeDistance < smallestEdgeDistance)
            {
                smallestEdgeDistance = edgeDistance;

                outerCandidates.Clear();

                outerCandidates.Add(direction);
            }
            else if (edgeDistance == smallestEdgeDistance)
            {
                outerCandidates.Add(direction);
            }
        }

        if (currentDirection == Vector2Int.zero)
        {
            return outerCandidates[Random.Range(0, outerCandidates.Count)];
        }

        Vector2Int clockwiseTurn = RotateClockwise(currentDirection);

        Vector2Int antiClockwiseTurn = RotateAnticlockwise(currentDirection);

        Vector2Int[] priority =
        {
            clockwiseTurn,
            currentDirection,
            antiClockwiseTurn,
            -currentDirection
        };

        foreach (Vector2Int preferred in priority)
        {
            if (outerCandidates.Contains(preferred))
            {
                return preferred;
            }
        }

        return outerCandidates[0];
    }

    private int GetDistanceFromMapEdge(Vector2Int grid)
    {
        int left = grid.x;

        int right = levelGenerator.FullColumns - 1 - grid.x;

        int top = grid.y;

        int bottom = levelGenerator.FullRows - 1 - grid.y;

        return Mathf.Min(left, right, top, bottom);
    }

    private Vector2Int RotateClockwise(Vector2Int direction)
    {
        return new Vector2Int(-direction.y , direction.x);
    }

    private Vector2Int RotateAnticlockwise(Vector2Int direction) 
    {
        return new Vector2Int(direction.y, -direction.x);
    }

    private Vector2Int ChooseSpawnExitDirection()
    {
        Vector2Int exitGrid = spawnArea.GetExitGrid(ghostNumber, levelGenerator);

        Vector2Int direction;

        if (currentGridPosition.x < exitGrid.x)
        {
            direction = GridRight;
        }
        else if (currentGridPosition.x > exitGrid.x)
        {
            direction = GridLeft;
        }
        else if (currentGridPosition.y < exitGrid.y)
        {
            direction = GridDown;
        }
        else if (currentGridPosition.y > exitGrid.y)
        {
            direction = GridUp;
        }
        else
        {
            return Vector2Int.zero;
        }

        Vector2Int nextGrid = currentGridPosition + direction;

        if (!levelGenerator.IsGhostWalkable(nextGrid, true))
        {
            Debug.LogError($"Ghost {ghostNumber}: Spawn path blocked. " + $"Current={currentGridPosition},  " + $"Next={nextGrid}, " + $"Tile={levelGenerator.GetTileAtGrid(nextGrid)}, " + $"Exit= {exitGrid}");

            return Vector2Int.zero;
        }

        return direction;
    }

    private Vector2Int FindDirectionToSpawnExit(Vector2Int target, bool allowInitialReverse)
    {
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        frontier.Enqueue(currentGridPosition);

        cameFrom[currentGridPosition] = currentGridPosition;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == target)
            {
                break;
            }

            foreach (Vector2Int direction in Directions)
            {
                if (!allowInitialReverse && current == currentGridPosition && currentDirection != Vector2Int.zero && direction == -currentDirection)
                {
                    continue;
                }

                Vector2Int next = current + direction;

                if (!levelGenerator.IsInsideGrid(next))
                {
                    continue;
                }

                if (IsTeleporterGrid(next))
                {
                    continue;
                }

                if (cameFrom.ContainsKey(next))
                {
                    continue;
                }

                cameFrom[next] = current;

                frontier.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(target))
        {
            return Vector2Int.zero;
        }

        Vector2Int pathCell = target;

        while (cameFrom[pathCell] != currentGridPosition)
        {
            Vector2Int previous = cameFrom[pathCell];

            if (previous == pathCell)
            {
                return Vector2Int.zero;
            }

            pathCell = previous;
        }

        return pathCell - currentGridPosition;  
    }

    private List<Vector2Int> GetValidDirections(bool allowGhostExitWall)
    {
        List<Vector2Int> valid = new List<Vector2Int>();

        foreach (Vector2Int direction in Directions)
        {
            Vector2Int next = currentGridPosition + direction;

            if (!levelGenerator.IsInsideGrid(next))
            {
                continue;
            }

            if(IsTeleporterGrid(next))
            {
                continue;
            }

            if (!levelGenerator.IsGhostWalkable(next, allowGhostExitWall))
            {
                continue;
            }

            if (hasExitedSpawnArea)
            {
                Vector3 nextWorld = levelGenerator.GridToWorld(next);

                if (spawnArea.ContainsWorldPosition(nextWorld))
                {
                    continue;
                }
            }

            valid.Add(direction);
        }
        return valid;   
    }

    private List<Vector2Int> GetDirectionsWithoutBackstep(List<Vector2Int> valid)
    {
        if (currentDirection == Vector2Int.zero || valid.Count <= 1)
        {
            return valid;
        }

        Vector2Int reverse = -currentDirection;

        List<Vector2Int> withoutReverse = new List<Vector2Int>();

        foreach (Vector2Int direction in valid)
        {
            if (direction != reverse)
            {
                withoutReverse.Add(direction);
            }
        }

        if (withoutReverse.Count > 0)
        {
            return withoutReverse;
        }

        return valid;
    }

    private bool IsTeleporterGrid(Vector2Int grid)
    {
        if (teleporters == null)
        {
            return false;
        }

        foreach (Teleporter teleporter in teleporters)
        {
            if (teleporter == null)
            {
                continue;
            }

            Vector2Int teleporterGrid = levelGenerator.WorldToGrid(teleporter.transform.position);

            if (grid == teleporterGrid)
            {
                return true;
            }
        }

        return false;
    }

    private void StartGridMove(Vector2Int direction)
    {
        currentDirection = direction;

        Vector2Int targetGrid = currentGridPosition + direction;

        lerpStartPosition = levelGenerator.GridToWorld(currentGridPosition);

        lerpTargetPosition = levelGenerator.GridToWorld(targetGrid);

        lerpDistance = Vector3.Distance(lerpStartPosition, lerpTargetPosition);

        lerpProgress = 0f;
        isLerping = true;
    }

    private void UpdateGridLerp()
    {
        float speed = GetCurrentMovementSpeed();

        if (lerpDistance <= 0f)
        {
            lerpProgress = 1f;
        }
        else
        {
            lerpProgress += (speed / lerpDistance) * Time.deltaTime;
        }

        float t = Mathf.Clamp01(lerpProgress);

        transform.position = Vector3.Lerp(lerpStartPosition, lerpTargetPosition, t);

        if (t >= 1f)
        {
            transform.position = lerpTargetPosition;

            currentGridPosition = levelGenerator.WorldToGrid(lerpTargetPosition);

            isLerping = false;

            if (!hasExitedSpawnArea)
            {
                Vector2Int exitGrid = spawnArea.GetExitGrid(ghostNumber, levelGenerator);

                if (currentGridPosition == exitGrid)
                {
                    hasExitedSpawnArea = true;
                }
            }
        }
    }

    private float GetCurrentMovementSpeed()
    {
        float normalSpeed = pacStudentController.MoveSpeed * normalMultiplier;

        switch (stateController.CurrentState)
        {
            case GhostState.Scared:
            case GhostState.Recovering:
            case GhostState.Dead:

                return normalSpeed * scaredMultiplier;

            case GhostState.Normal:
            default:

                return normalSpeed;
        }
    }

    private void BeginDeadMovement()
    {
        isLerping = false;

        deadArrivalHandled = false;

        deadStartPosition = transform.position;

        deadTargetPosition = startingWorldPosition;

        deadDistance = Vector3.Distance(deadStartPosition, deadTargetPosition);

        deadProgress = 0f;
    }

    private void UpdateDeadMovement()
    {
        if (deadArrivalHandled)
        {
            return;
        }

        float speed = GetCurrentMovementSpeed();

        if (deadDistance <= 0.001f)
        {
            deadProgress = 1f;
        }
        else
        {
            deadProgress += (speed / deadDistance) * Time.deltaTime;
        }

        float t = Mathf.Clamp01(deadProgress);

        transform.position = Vector3.Lerp(deadStartPosition, deadTargetPosition, t);

        if (t >= 1f)
        {
            transform.position = deadTargetPosition;

            currentGridPosition = levelGenerator.WorldToGrid(transform.position);

            currentDirection = Vector2Int.zero;

            hasExitedSpawnArea = false;

            deadArrivalHandled = true;

            gameManager.ReviveGhost(stateController);
        }
    }
}
