using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{

   private int[,] levelMap = 
   { 
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
   };

    [Header("Level Piece Prefabs")]
    [SerializeField] private GameObject outsideCornerPrefab;
    [SerializeField] private GameObject insideCornerPrefab;
    [SerializeField] private GameObject outsideWallPrefab;
    [SerializeField] private GameObject insideWallPrefab;
    [SerializeField] private GameObject pelletPrefab;
    [SerializeField] private GameObject powerPelletPrefab;
    [SerializeField] private GameObject tJunctionPrefab;
    [SerializeField] private GameObject ghostExitWallPrefab;


    [Header("Scene References")]
    [SerializeField] private GameObject manualLevel;
    [SerializeField] private Camera gameCamera;


    [Header("Generation Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float cameraPadding = 1f;

    [Header("Sprite Base Rotations")]

    [Tooltip("Rotation at which the outside corner looks like ┌")]
    [SerializeField] private float outsideCornerBaseRotation = 0f;

    [Tooltip("Rotation at which the inside corner looks like ┌")]
    [SerializeField] private float insideCornerBaseRotation = 0f;

    [Tooltip("Rotation at which the outside wall which is vertical")]
    [SerializeField] private float outsideWallVerticalRotation = 0f;

    [Tooltip("Rotation at which the inside wall which is vertical")]
    [SerializeField] private float insideWallVerticalRotation = 0f;

    [Tooltip("Rotation at which the T junction looks like ┬")]
    [SerializeField] private float tJunctionBaseRotation = 0f;

    [Tooltip("Rotation at which the ghost exit wall us vertical")]
    [SerializeField] private float ghostExitVerticalRotation = 0f;



    private const int Up = 1;
    private const int Down = 4;
    private const int Left = 8;
    private const int Right = 2;

    private readonly int[] directions =
    {
        Up,
        Down,
        Left,
        Right,
    };

    private int[,] fullMap;
    private int[,] solvedConnections;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (manualLevel != null)
        {
            manualLevel.SetActive(false);
            Destroy(manualLevel);
        }

        BuildFullMap();

        if (!SolveWallConnections())
        {
            Debug.LogError("LevelGenerator could not determine a valid wall layout.");

            return;
        }

        GenerateLevel();
        AdjustCamer();
    }

    private void BuildFullMap()
    {
        int rows = levelMap.GetLength(0);
        int columns = levelMap.GetLength(1);

        int fullRows = (rows * 2) - 1;
        int fullColumns = columns * 2;

        fullMap = new int[fullRows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {

                int value = levelMap[row, column];

                fullMap[row, column] = value;

                int mirroredColumn = fullColumns - 1 - column;

                fullMap[row, mirroredColumn] = value;

                if (row < rows - 1)
                {
                    int mirroredRow = fullRows - 1 - row;

                    fullMap[mirroredRow, column] = value;

                    fullMap[mirroredRow, mirroredColumn] = value;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
