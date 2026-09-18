using UnityEngine;
using System.Collections.Generic;
using Mono.Cecil;

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
        AdjustCamera();
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


    private bool SolveWallConnections()
    {
        int rows = fullMap.GetLength(0);
        int columns = fullMap.GetLength(1);

        List<int>[,] possibilities = new List<int>[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                possibilities[row, column] = GetPossibleConnections(fullMap[row, column]);
            }
        }

        if (fullMap[0, 0] == 1)
        {
            possibilities[0, 0].Clear();

            possibilities[0, 0].Add(Right | Down);
        }

        if (!SolvePossibilities(ref possibilities))
        {
            return false;
        }

        solvedConnections = new int[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (possibilities[row, column].Count == 1)
                {
                    solvedConnections[row, column] = possibilities[row, column][0];
                }
            }
        }

        return true;
    }


    private List<int> GetPossibleConnections(int tile)
    {
        List<int> result = new List<int>();

        switch (tile)
        {
            // This is for outside corner as well as inside corner
            case 1:
            case 3:

                result.Add(Up | Right);
                result.Add(Right | Down);
                result.Add(Down | Left);
                result.Add(Left | Up);

                break;

            //This is for straight walls or ghost exit
            case 2:
            case 4:
            case 8:

                result.Add(Up | Down);
                result.Add(Left | Right);

                break;

            //This is for T junction
            case 7:

                result.Add(Left | Right | Down);
                result.Add(Up |  Down | Left);
                result.Add(Up | Left | Right);
                result.Add(Up | Right | Down);

                break;

            default:

                result.Add(0);
                break;
        }
        return result;
    }


    private bool SolvePossibilities(ref List<int>[,] possibilities)
    {
        if (!Propagate(possibilities))
        {
            return false;
        }

        int bestRow = -1;
        int bestColumn = -1;
        int bestCount = int.MaxValue;

        int rows = possibilities.GetLength(0);
        int columns = possibilities.GetLength(1);

        for (int row  = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int count = possibilities[row, column].Count;

                if (count > 1 && count < bestCount)
                {
                    bestCount = count;
                    bestRow = row;
                    bestColumn = column;
                }
            }
        }

        if (bestRow == -1)
        {
            return true;
        }

        List<int> choices = new List<int>(possibilities[bestRow, bestColumn]);

        foreach (int choice in choices)
        {
            List<int>[,] attempt = ClonePossibilities(possibilities);

            attempt[bestRow, bestColumn].Clear();
            attempt[bestRow, bestColumn].Add(choice);

            if (SolvePossibilities(ref attempt))
            {
                possibilities = attempt;
                return true;
            }
        }

        return false;  
    }


    private bool Propagate(List<int>[,] possibilities)
    {
        bool changed;

        int rows = possibilities.GetLength(0);
        int columns = possibilities.GetLength(1);

        do
        {
            changed = false;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (!IsWallTile(fullMap[row, column]))
                    {
                        continue;
                    }

                    List<int> current = possibilities[row, column];

                    for (int i = current.Count - 1; i >= 0; i--)
                    {
                        if (!ConnectionPatternIsPossible(row, column, current[i], possibilities))
                        {
                            current.RemoveAt(i);
                            changed = true;
                        }
                    }

                    if(current.Count == 0)
                    {
                        return false;
                    }
                }
            }
        }

        while (changed);

        return true;
    }

    private bool ConnectionPatternIsPossible(int row, int column, int pattern, List<int>[,] possibilities)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            int direction = directions[i]; 

            GetDirectionOffset(direction, out int rowOffset, out int columnOffset);

            int neighbourRow = row + rowOffset;

            int neighbourColumn = column + columnOffset;

            bool thisConnects = (pattern & direction) != 0;

            if (!IsInsideMap(neighbourRow, neighbourColumn))
            {
                if (thisConnects)
                {
                    return false;
                }

                continue;
            }

            int currentTile = fullMap[row, column];

            int neighbourTile = fullMap[neighbourRow, neighbourColumn];

            if (!TilesCanConnect(currentTile, neighbourTile))
            {
                if (thisConnects)
                {
                    return false;
                }

                continue;
            }

            int oppositeDirection = Opposite(direction);

            bool supported = false;

            foreach (int neighbourPattern in possibilities[neighbourRow, neighbourColumn])
            {
                bool neighbourConnects = (neighbourPattern & oppositeDirection) != 0;

                if (thisConnects == neighbourConnects)
                {
                    supported = true;
                    break;
                }
            }

            if (!supported)
            {
                return false;
            }
        }
        
        return true;
    }

    private List<int>[,] ClonePossibilities(List<int>[,] original)
    {
        int rows = original.GetLength(0);
        int columns = original.GetLength(1);

        List<int>[,] copy = new List<int>[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                copy[row, column] = new List<int>(original[row, column]);
            }
        }

        return copy;
    }

    private bool IsWallTile(int tile)
    {
        return tile == 1
            || tile == 2
            || tile == 3
            || tile == 4
            || tile == 5
            || tile == 6
            || tile == 7
            || tile == 8;
    }

    private bool TilesCanConnect(int first, int second)
    {
        if (!IsWallTile(first) || !IsWallTile(second))
        {
            return false;
        }

        if (first == 7 || second == 7)
        {
            return true;
        }

        if (first == 8)
        {
            return second == 3
                || second == 4
                || second == 8;
        }

        if (second == 8)
        {
            return first == 3
                || first == 4
                || first == 8;
        }

        bool firstOutside = first == 1 || first == 2;

        bool secondOutside = second == 1 || second == 2;

        bool firstInside = first == 3 || first == 4;

        bool secondInside = second == 3 || second == 4; 

        return (firstOutside && secondOutside) || (firstInside && secondInside);
    }

    private bool IsInsideMap (int row, int column)
    {
        return row >= 0 && row < fullMap.GetLength(0)
            && column >= 0
            && column < fullMap.GetLength(1);
    }

    private int Opposite (int direction)
    {
        switch (direction)
        {
            case Up:
                return Down;

            case Right:
                return Left;

            case Down:
                return Up;

            default:
                return Right;
        }
    }

    private void GetDirectionOffset (int direction, out int rowOffset, out int columnOffset)
    {
        rowOffset = 0;
        columnOffset = 0;

        switch(direction)
        {
            case Up:
                rowOffset = -1;
                break;

            case Right:
                columnOffset = 1;
                break;

            case Down:
                rowOffset = 1;
                break;

            case Left:
                columnOffset = -1;
                break;
        }
    }


    private void GenerateLevel()
    {
        int sourceRows = levelMap.GetLength(0);

        int sourceColumns = levelMap.GetLength(1);

        int fullRows = (sourceRows * 2) - 1;

        int fullColumns = sourceColumns * 2;

        GameObject generatedLevel = new GameObject("GeneratedLevel");


        //Top-Left:
        Transform topLeft = CreateQuadrant(generatedLevel.transform, "TopLeft");

        GenerateQuadrant(topLeft, true);

        //Top-Right:
        Transform topRight = CreateQuadrant(generatedLevel.transform, "TopRight");

        topRight.localPosition = new Vector3((fullColumns - 1) * tileSize, 0f, 0f);

        topRight.localScale = new Vector3(-1f, 1f, 1f);

        GenerateQuadrant(topRight, true);


        //Bottom-Left:
        Transform bottomLeft = CreateQuadrant(generatedLevel.transform, "BottomLeft");

        bottomLeft.localPosition = new Vector3(0f, -(fullRows - 1) * tileSize, 0f);

        bottomLeft.localScale = new Vector3(1f, -1f, 1f);

        GenerateQuadrant(bottomLeft, false);

        //Bottom-Right:
        Transform bottomRight = CreateQuadrant(generatedLevel.transform, "BottomRight");

        bottomRight.localPosition = new Vector3((fullColumns - 1) * tileSize, -(fullRows - 1) * tileSize, 0f);

        bottomRight.localScale = new Vector3(-1f, -1f, 1f);

        GenerateQuadrant(bottomRight, false);
    }

    private Transform CreateQuadrant(Transform parent, string quadrantName)
    {
        GameObject quadrant = new GameObject(quadrantName);

        quadrant.transform.SetParent(parent, false);

        return quadrant.transform;
    }

    private void GenerateQuadrant(Transform parent, bool includeBottomRow)
    {
        int rows = levelMap.GetLength(0);
        int columns = levelMap.GetLength(1);

        int rowsToGenerate = includeBottomRow ? rows : rows - 1;

        for (int row  = 0; row < rowsToGenerate; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int tile = levelMap[row, column];

                if (tile == 0)
                {
                    continue;
                }

                GameObject prefab = GetPrefab(tile);

                if (prefab == null)
                {
                    Debug.LogError("No prefab assigned for tile " + tile);
                    
                    continue;
                }

                GameObject piece = Instantiate(prefab, parent);

                piece.name = "R" + row.ToString("00") + "_C" + column.ToString("00") + "_" + GetTileName(tile);

                piece.transform.localPosition = new Vector3(column * tileSize, -row * tileSize, 0f);

                float rotation = GetRotation(tile, solvedConnections[row, column]);

                piece.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

                piece.transform.localScale = Vector3.one;

            }
        }
    }

    private GameObject GetPrefab(int tile)
    {
        switch (tile)
        {
            case 1:
                return outsideCornerPrefab;

            case 2:
                return outsideWallPrefab;

            case 3:
                return insideCornerPrefab;

            case 4:
                return insideWallPrefab;

            case 5:
                return pelletPrefab;

            case 6:
                return powerPelletPrefab;

            case 7:
                return tJunctionPrefab;

            case 8:
                return ghostExitWallPrefab;

            default:
                return null;
        }
    }


    private float GetRotation(int tile, int connections)
    {
        switch (tile)
        {
            case 1:
                return outsideCornerBaseRotation + GetCornerRotation(connections);

            case 2:
                return outsideWallVerticalRotation + GetStraightRotation(connections);

            case 3:
                return insideCornerBaseRotation + GetCornerRotation(connections);

            case 4:
                return insideWallVerticalRotation + GetStraightRotation(connections);

            case 7:
                return tJunctionBaseRotation + GetTJunctionRotation(connections);

            case 8:
                return ghostExitVerticalRotation + GetStraightRotation(connections);

            default:
                return 0f;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
