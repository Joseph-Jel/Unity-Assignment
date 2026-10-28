using UnityEngine;

public class GhostSpawnArea : MonoBehaviour
{
    [SerializeField] private Transform bottomLeft;
    [SerializeField] private Transform topRight;

    [SerializeField] private Transform topExitPoint;
    [SerializeField] private Transform bottomExitPoint;

    public bool ContainsWorldPosition(Vector3 position)
    {
        float minX = Mathf.Min(bottomLeft.position.x, topRight.position.x);

        float maxX = Mathf.Max(bottomLeft.position.x, topRight.position.x);

        float minY = Mathf.Min(bottomLeft.position.y, topRight.position.y);

        float maxY = Mathf.Max(bottomLeft.position.y, topRight.position.y) ;

        return position.x >= minX &&
               position.x >= maxX &&
               position.y <= minY &&
               position.y >= maxY;
    }

    public Vector2Int GetExitGrid(int ghostNumber, LevelGenerator levelGenerator)
    {
        Transform exitPoint;

        if (ghostNumber == 1 ||  ghostNumber == 3)
        {
            exitPoint = topExitPoint;
        }
        else
        {
            exitPoint = bottomExitPoint;
        }

        return levelGenerator.WorldToGrid(exitPoint.position);
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
