using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class LevelGridManager : MonoBehaviour
{
    private GridData[,] quadrantData = new GridData[15, 14];
    public static GridData[,] gridData = new GridData[2 * 14 + 1, 2 * 14];

    private void Awake()
    {
        //Debug.Log("I AM AWAKE");

        GridData[,] centerline;
        quadrantData = ReadQuadrantData(out centerline);
        gridData = GenerateGridFromQuadrantData(quadrantData, centerline);
    }
    
    public static Vector2Int GetCoordinatesFromPoint(Vector2 point)
    {
        int rows = gridData.GetLength(0);
        int cols = gridData.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (Vector2.Distance(gridData[r, c].position, point) < 0.1f)
                {
                    return new Vector2Int(r, c);
                }
            }
        }
        
        return Vector2Int.zero;
    }

    GridData[,] ReadQuadrantData(out GridData[,] _centerLine)
    {
        int index = 0;
        GridData[,] _quadrantData = new GridData[14, 14];
        _centerLine = new GridData[1, 14];
        Transform quadrant = transform.GetChild(0);

        GridData _currGD;
        for (int row = 0; row < _quadrantData.GetLength(0) + 1; row++)
        {
            for (int col = 0; col < _quadrantData.GetLength(1); col++)
            {
                //Debug.Log(row + " " + col);
                _currGD = new GridData();
                //Debug.Log(index);
                Transform child = quadrant.GetChild(index)?.childCount > 0? quadrant.GetChild(index).GetChild(0) : null;

                if (child == null)
                {
                    _currGD.walkable = GridData.Walkable.walkable;
                }
                else
                {
                    int tileType;
                    string[] tileName = child.name.Split('_');
                    int.TryParse(tileName[1], out tileType);
                    //Debug.Log(tileType);
                    switch (tileType)
                    {
                        case 5 or 6:
                            _currGD.walkable = GridData.Walkable.walkable;
                            break;

                        case 8:
                            _currGD.walkable = GridData.Walkable.ghostWall;
                            break;
                        
                        case 1 or 2:
                            _currGD.wallType = GridData.WallType.outer;
                           //Debug.Log(tileType + " " + _currGD.wallType);
                            goto default;
                            
                        case 3 or 4:
                            _currGD.wallType = GridData.WallType.inner;
                            goto default;

                        default:
                            _currGD.walkable = GridData.Walkable.unwalkable;
                            break;
                    }
                }
                //Debug.Log(_quadrantData[row, col].walkable);
                _currGD.position = quadrant.GetChild(index).position;

                if (row > _quadrantData.GetLength(0) - 1)
                {
                    _centerLine[0, col] = new GridData();
                    _centerLine[0, col] = _currGD;
                }
                else
                {
                    _quadrantData[row, col] = new GridData();
                    _quadrantData[row, col] = _currGD;
                }
                index++;
            }
        }

        return _quadrantData;
    }

    GridData[,] GenerateGridFromQuadrantData(GridData[,] _quadrantData, GridData[,] _centerLine)
    {
        int rows = 2 * 14 + 1;
        int cols = 2 * 14;

        int quadRows = _quadrantData.GetLength(0);
        int quadCols = _quadrantData.GetLength(1);
        
        GridData[,] fullGrid = new GridData[rows, cols];

        /*//top left
        for (int r = 0; r < quadRows; r++)
        {
            for (int c = 0; c < quadCols; c++)
            {
                fullGrid[r, c] = new GridData
                {
                    walkable = _quadrantData[r, c].walkable,
                    position = _quadrantData[r, c].position
                };
            }
        }
        
        //top right
        Debug.Log(quadCols + " " + cols);
        for (int r = 0; r < quadRows; r++)
        {
            for (int c = 0; c < quadCols; c++)
            {
                int mirroredCol = cols - 1 - c;
                /*Debug.Log(fullGrid.GetLength(0) + " " + fullGrid.GetLength(1));
                Debug.Log(r + " " + c);#1#
                
                fullGrid[r, mirroredCol] = new GridData 
                {
                    walkable = _quadrantData[r, c].walkable,
                    position = new Vector2(-_quadrantData[r, c].position.x, _quadrantData[r, c].position.y)
                };
            }
        }

        //bottom half
        for (int r = 0; r < quadRows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int mirroredRows = rows - 1 - r;
                Debug.Log(fullGrid.GetLength(0) + " " + fullGrid.GetLength(1));
                Debug.Log(mirroredRows + " " + c);
                
                fullGrid[mirroredRows, c] = new GridData 
                {
                    walkable = fullGrid[r, c].walkable,
                    position = new Vector2(fullGrid[r, c].position.x, -fullGrid[r, c].position.y)
                };
            }
        }*/
        
        //top half
        for (int r = 0; r < quadRows; r++)
        {
            for (int c = 0; c < quadCols; c++)
            {
                fullGrid[r, c] = new GridData
                {
                    walkable = _quadrantData[r, c].walkable,
                    position = _quadrantData[r, c].position,
                    wallType = _quadrantData[r, c].wallType
                };

                int mirroredCol = cols - 1 - c;
                fullGrid[r, mirroredCol] = new GridData
                {
                    walkable = _quadrantData[r, c].walkable,
                    position = new Vector2(-_quadrantData[r, c].position.x, _quadrantData[r, c].position.y),
                    wallType = _quadrantData[r, c].wallType
                };
            }
        }
        
        //center line
        for (int c = 0; c < quadCols; c++)
        {
            //Debug.Log(quadRows + " " + c);
            //Debug.Log(fullGrid.GetLength(00) + " " + fullGrid.GetLength(01));
            fullGrid[quadRows, c] = new GridData
            {
                walkable = _centerLine[0, c].walkable,
                position =_centerLine[0, c].position,
                wallType = _centerLine[0, c].wallType
            };
            
            fullGrid[quadRows, cols - 1 - c] = new GridData
            {
                walkable = _centerLine[0, c].walkable,
                position = new Vector2(-_centerLine[0, c].position.x, _centerLine[0, c].position.y),
                wallType = _centerLine[0, c].wallType
            };
        }

        //bottom half
        for (int r = 0; r < quadRows; r++)
        {
            int mirroredRow = rows - 1 - r;
            for (int c = 0; c < cols; c++)
            {
                fullGrid[mirroredRow, c] = new GridData
                {
                    walkable = fullGrid[r, c].walkable,
                    position = new Vector2(fullGrid[r, c].position.x, -fullGrid[r, c].position.y + 0.08f),
                    wallType = fullGrid[r, c].wallType
                };
            }
        }
        
        return fullGrid;
    }

    private void OnDrawGizmos()
    {
        if (gridData == null)
            return;
        for (int col = 0; col < gridData.GetLength(1); col++)
        {
            for (int row = 0; row < gridData.GetLength(0); row++)
            {
                if (gridData[row, col] == null) continue;
                switch (gridData[row, col].walkable)
                {
                    case GridData.Walkable.walkable:
                        Gizmos.color = Color.green;
                        break;
                    case GridData.Walkable.unwalkable:
                        Gizmos.color = Color.red;
                        break;
                    case GridData.Walkable.ghostWall:
                        Gizmos.color = Color.blue;
                        break;
                }

                Gizmos.DrawWireCube(gridData[row, col].position, Vector3.one * 0.1f);
            }
        }
           
    }

    void OnDrawGizmosSelected()
    {
        for (int row = 0; row < 14; row++)
        {
            Gizmos.color = new Color(UnityEngine.Random.value,  UnityEngine.Random.value, UnityEngine.Random.value);
            for (int col = 0; col < 14; col++)
            {
                Gizmos.DrawWireCube(quadrantData[row, col].position, Vector3.one * 0.1f);
            }
        }
    }
}


[System.Serializable]
public class GridData
{
    public Vector2 position;
    public enum Walkable
    {
        walkable,
        unwalkable,
        ghostWall
    }
    public Walkable walkable;

    public enum WallType
    {
        none,
        inner,
        outer,
    }
    public WallType wallType;
}
