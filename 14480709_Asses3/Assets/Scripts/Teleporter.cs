using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Vector2Int destinationGrid;
    [SerializeField] private Vector2Int inwardDirection;

    public Vector2Int DestinationGrid
    {
        get { return destinationGrid; }
    }
    public Vector2Int InwardDirection
    {
        get { return inwardDirection; }
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
