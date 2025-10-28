using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGridManager : MonoBehaviour
{
    private GridData[,] quadrantData = new GridData[15, 14];
    public static GridData[,] gridData = new GridData[2 * 14, 2 * 14 + 1];

    private void Awake()
    {
        //Debug.Log("I AM AWAKE");
        
        quadrantData = ReadQuadrantData();
    }

    GridData[,] ReadQuadrantData()
    {
        int index = 0;
        GridData[,] _quadrantData = new GridData[15, 14];
        Transform quadrant = transform.GetChild(0);
        for (int col = 0; col < _quadrantData.GetLength(1); col++)
        {
            for (int row = 0; row < _quadrantData.GetLength(0); row++)
            {
                _quadrantData[row, col] = new GridData();
                //Debug.Log(index);
                Transform child = quadrant.GetChild(index)?.childCount > 0? quadrant.GetChild(index).GetChild(0) : null;

                if (child == null)
                {
                    _quadrantData[row, col].walkable = GridData.Walkable.walkable;
                }
                else
                {

                    int tileType;
                    string[] tileName = child.name.Split('_');
                    int.TryParse(tileName[1], out tileType);
                    Debug.Log(tileType);
                    switch (tileType)
                    {
                        case 5 or 6:
                            _quadrantData[row, col].walkable = GridData.Walkable.walkable;
                            break;

                        case 8:
                            _quadrantData[row, col].walkable = GridData.Walkable.ghostWall;
                            break;

                        default:
                            _quadrantData[row, col].walkable = GridData.Walkable.unwalkable;
                            break;
                    }
                }
                
                //Debug.Log(_quadrantData[row, col].walkable);
                _quadrantData[row, col].position = quadrant.GetChild(index).position;
                index++;
            }
        }

        return _quadrantData;
    }

    /*GridData[,] GenerateGridFromQuadrantData(GridData[,] quadrantData)
    {

    }*/

    private void OnDrawGizmos()
    {
        if (quadrantData == null)
            return;
        for (int col = 0; col < quadrantData.GetLength(1); col++)
        {
            for (int row = 0; row < quadrantData.GetLength(0); row++)
            {
                switch (quadrantData[row, col].walkable)
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
}
