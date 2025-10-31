using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GhostController : MonoBehaviour
{
    public Direction defaultDirection;
    Animator animator;
    GhostsManager ghostManager;
    PacStudentController player;
    
    GridData[,] gridData;
    private Vector2Int currentCoordinates;
    private bool isLerping;
    Vector2Int startPosition;

    [SerializeField]
    private float speed = 1f;

    private Vector2 center;

    public enum Behaviour
    {
        Ghost1,
        Ghost2,
        Ghost3,
        Ghost4
    };
    public Behaviour behaviour;
    
    public enum Direction
    {
        up,
        down,
        left,
        right
    };
    
    public bool isDead = false;

    private Direction _currDir;
    Direction currentDirection
    {
        get { return _currDir; }
        set
        {
            _currDir = value;
            animator.SetInteger("Direction", (int)_currDir);
        }
    }

    private Direction newDir;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentDirection = defaultDirection;
        gridData = LevelGridManager.gridData;
        currentCoordinates = LevelGridManager.GetCoordinatesFromPoint(transform.position);
        startPosition = currentCoordinates;
        transform.position = gridData[currentCoordinates.x, currentCoordinates.y].position;
        center = GameObject.FindGameObjectWithTag("Center").transform.position;
        player = GameManager.instance.Player;
        
        StartCoroutine(Move());
    }
    
    Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.up:
                return Direction.down;
            case Direction.down:
                return Direction.up;
            case Direction.left:
                return Direction.right;
            case Direction.right:
                return Direction.left;
        }
        
        return Direction.up;
    }

    IEnumerator Move()
    {
        while (true)
        {
            if (!GameManager.instance.gameReady) yield return null;

            if (!isLerping)
            {
                Vector2Int newCoordinates = ChooseNextPos();
                ApplyMoveAnimation(gridData[newCoordinates.x, newCoordinates.y].position-gridData[currentCoordinates.x, currentCoordinates.y].position);
                yield return LerpGhost(newCoordinates);
            }
        }
    }
    
    IEnumerator LerpGhost(Vector2Int targetCoordinates)
    {
        isLerping = true;
        float t = 0;
        float duration = 1/speed; //1 tile

        Vector2 startPos = gridData[currentCoordinates.x, currentCoordinates.y].position;
        Vector2 endPos = gridData[targetCoordinates.x, targetCoordinates.y].position;
        
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t/duration);
            yield return null;
        }

        currentCoordinates = targetCoordinates;
        isLerping = false;
    }

    void ApplyMoveAnimation(Vector2 dir)
    {
        if (dir.normalized == Vector2.up)
            currentDirection = Direction.up;
        else if (dir.normalized == Vector2.down)
            currentDirection = Direction.down;
        else if (dir.normalized == Vector2.left)
            currentDirection = Direction.left;
        else if (dir.normalized == Vector2.right)
            currentDirection = Direction.right;
            
        animator.SetInteger("Direction", (int)currentDirection);
    }
    
    Vector2Int ChooseNextPos()
    {
        List<Vector2Int> possibleDirections = new List<Vector2Int>(GetPriorityCoords(GetPossibleDirections()));
        
        if (behaviour == Behaviour.Ghost4)
            Debug.Log(possibleDirections.Count);
        if (possibleDirections.Count == 1)
            return possibleDirections[0];

        switch (behaviour)
        {
            case Behaviour.Ghost1: // equidistant or further from player
                Vector2Int furthestCoords = Vector2Int.zero;
                float furthestDist = 0;
                for (int i = 0; i < possibleDirections.Count - 1; i++)
                {
                    if (GetDistance(possibleDirections[i]) > furthestDist)
                    {
                        furthestCoords = possibleDirections[i];
                        furthestDist = GetDistance(possibleDirections[i]);
                    }
                }
                return furthestCoords;
            case Behaviour.Ghost2: // equidistant or nearer to player
                Vector2Int nearestCoords = Vector2Int.zero;
                float nearestDist = 1000f;
                for (int i = 0; i < possibleDirections.Count - 1; i++)
                {
                    if (GetDistance(possibleDirections[i]) < nearestDist)
                    {
                        nearestCoords = possibleDirections[i];
                        nearestDist = GetDistance(possibleDirections[i]);
                    }
                }
                return nearestCoords;
            case Behaviour.Ghost3: // random 
                int randIndex = Random.Range(0, possibleDirections.Count - 1);
                return possibleDirections[randIndex];
            case Behaviour.Ghost4: // hug outer walls
                Vector2Int furthestFromCenter = Vector2Int.zero;
                float furthest = 0;
                for (int i = 0; i < possibleDirections.Count-1; i++)
                {
                    if (GetDistance(possibleDirections[i], center) > furthest)
                    {
                        furthestFromCenter = possibleDirections[i];
                        furthest = GetDistance(possibleDirections[i], center);
                    }
                }
                return furthestFromCenter;
        }
        return Vector2Int.zero;
    }

    float GetDistance(Vector2Int newCoordinates, Vector2 position = new Vector2())
    {
        if (position.magnitude > 0)
            return Vector2.Distance(
            position, gridData[newCoordinates.x, newCoordinates.y].position);
        
        return Vector2.Distance(
            gridData[player.GetCoordinates().x, player.GetCoordinates().y].position, 
            gridData[newCoordinates.x, newCoordinates.y].position);
    }

    List<Vector2Int> GetPriorityCoords(List<Direction> allDirs)
    {
        List<Direction> priorityDirs = new List<Direction>(allDirs);
        Direction opposite = GetOppositeDirection(currentDirection);
        priorityDirs = priorityDirs.OrderBy(d => d == opposite ? 1:0).ToList();
        
        List<Vector2Int> newCoords = new List<Vector2Int>();
        for (int i = 0; i < priorityDirs.Count; i++)
        {
            newCoords.Add(GetCoordinatesFromDir(priorityDirs[i]));
        }
        return newCoords;
    }

    List<Direction> GetPossibleDirections()
    {
        // check all directions
        // find all walkable
        List<Direction>  possibleDirections = new List<Direction>();
        Vector2Int newCoordinates;
        for (int i = 0; i < 4; i++)
        {
            if (IsNewDirValid((Direction)i, out newCoordinates))
                possibleDirections.Add((Direction)i);
        }
        
        return possibleDirections;
    }

    bool IsNewDirValid(Direction dir, out Vector2Int newCoordinates)
    {
        newCoordinates = GetCoordinatesFromDir(dir);

        // if prev coordinates was inside box, include ghost walls
        return gridData[newCoordinates.x, newCoordinates.y].walkable.Equals(GridData.Walkable.walkable);
    }

    Vector2Int GetCoordinatesFromDir(Direction dir)
    {
        Vector2Int newCoordinates = currentCoordinates;
        switch (dir)
        {
            case Direction.up:
                newCoordinates.x -= 1;
                break;
            case Direction.left:
                newCoordinates.y -= 1;
                break;
            case Direction.down:
                newCoordinates.x += 1;
                break;
            case Direction.right:
                newCoordinates.y += 1;
                break;
        }
        
        return newCoordinates;
    }

    public void SetGhostManager(GhostsManager man)
    {
        ghostManager = man;
    }

    public void Die()
    {
        isDead = true;
        StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        animator.SetLayerWeight(3, 1);
        
        yield return new WaitForSeconds(3f);
        isDead = false;
        
        animator.SetLayerWeight(3, 0);
    }
}
