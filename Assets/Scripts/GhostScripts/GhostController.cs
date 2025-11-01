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
    private Vector2Int startPosition;
    private Vector2Int ghostWallPos;
    
    public float speed = 1f;

    private Vector2 center;

    public enum Behaviour
    {
        Ghost1, // run away
        Ghost2, // chase
        Ghost3, // random
        Ghost4  // hug walls
    };
    
    Behaviour currentBehaviour;
    public Behaviour defaultBehaviour;
    
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

    private int touchingOuterWall = 0;     //1 = right, 2 = left

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentBehaviour = defaultBehaviour;
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
            if (GameManager.instance.gameReady)
            {
                if (!isLerping && !isDead)
                {
                    Vector2Int newCoordinates = ChooseNextPos();
                    ApplyMoveAnimation(gridData[newCoordinates.x, newCoordinates.y].position-gridData[currentCoordinates.x, currentCoordinates.y].position);
                    yield return LerpGhost(newCoordinates);
                }
            }
            
            yield return null;
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
        List<Direction> allDirs = new List<Direction>(GetPossibleDirections());
        List<Vector2Int> possibleDirections = new List<Vector2Int>(PrioritiseFrontCoords(allDirs));
        if (ghostManager.IsInsideGhostBox(transform.position))
        {
           //Debug.Log("inside");
           return GetDirToGhostWall(possibleDirections);
        }
        /*if (behaviour == Behaviour.Ghost4)
            Debug.Log(possibleDirections.Count);*/
        
        
        if (possibleDirections.Count == 1)
            return possibleDirections[0];

        switch (currentBehaviour)
        {
            case Behaviour.Ghost1: // equidistant or further from player
                Vector2Int furthestCoords = Vector2Int.zero;
                float furthestDist = 0;
                for (int i = 0; i < possibleDirections.Count; i++)
                {
                    if (GetDirFromCoordinates(possibleDirections[i]) == GetOppositeDirection(currentDirection))
                        continue;
                    if (GetDistance(possibleDirections[i], player.GetCoordinates()) > furthestDist)
                    {
                        furthestCoords = possibleDirections[i];
                        furthestDist = GetDistance(possibleDirections[i], player.GetCoordinates());
                    }
                }
                return furthestCoords;
            case Behaviour.Ghost2: // equidistant or nearer to player
                return GetNearestCoords(possibleDirections.ToArray(), player.GetCoordinates());
            case Behaviour.Ghost3: // random 
                int randIndex = Random.Range(0, possibleDirections.Count - 1);
                return possibleDirections[randIndex];
            case Behaviour.Ghost4: // hug outer walls
                if (touchingOuterWall > 0 || IsTouchingOuterWall(allDirs.ToArray()))
                {
                    //Debug.Log("Touch");
                    possibleDirections = new List<Vector2Int>(PrioritiseWall(allDirs));
                }
                return possibleDirections[0];
        }
        return Vector2Int.zero;
    }

    Vector2Int GetNearestCoords(Vector2Int[] possibleDirections, Vector2Int pos)
    {
        Vector2Int nearestCoords = Vector2Int.zero;
        float nearestDist = 1000f;
        for (int i = 0; i < possibleDirections.Length; i++)
        {
            if (GetDirFromCoordinates(possibleDirections[i]) == GetOppositeDirection(currentDirection))
                continue;
            if (GetDistance(possibleDirections[i], pos) < nearestDist)
            {
                nearestCoords = possibleDirections[i];
                nearestDist = GetDistance(possibleDirections[i], pos);
            }
        }
        return nearestCoords;
    }

    bool IsTouchingOuterWall(Direction[] possibleDirections)
    {
        List<Direction> impossibleDirs = new List<Direction>();
        for (int i = 0; i < 4; i++)
        {
            if (possibleDirections.Contains((Direction)i)) continue;
            //Debug.Log((Direction)i);
            impossibleDirs.Add((Direction)i);
        }
        
        foreach (Direction dir in impossibleDirs)
        {
            Vector2Int coords = GetCoordinatesFromDir(dir);
            GridData data = gridData[coords.x, coords.y];
            //Debug.Log(gridData[coords.x, coords.y].wallType);
            if (data.wallType == GridData.WallType.outer)
            {
                if (dir ==  Direction.up && currentDirection == Direction.up|| dir == Direction.down && currentDirection == Direction.down) continue;
                touchingOuterWall = dir == LocalRightDir() ? 1 : 2;
                //Debug.Log(name+ " "  + dir);
                return true; 
            }
        }
        return false;
    }

    float GetDistance(Vector2Int newCoordinates, Vector2Int position)
    {
            return Vector2.Distance(
            gridData[position.x, position.y].position, 
            gridData[newCoordinates.x, newCoordinates.y].position);
    }
    
    Direction LocalRightDir()
    {
        switch (currentDirection)
        {
            case Direction.up:
                return Direction.right;
            case Direction.down:
                return Direction.left;
            case Direction.left:
                return Direction.up;
            case Direction.right:
                return Direction.down;
        }
        
        return Direction.up;
    }

    Direction LocalLeftDir()
    {
        switch (currentDirection)
        {
            case Direction.up:
                return Direction.left;
            case Direction.down:
                return Direction.right;
            case Direction.left:
                return Direction.down;
            case Direction.right:
                return Direction.up;
        }
        
        return Direction.up;
    }
    
    /*List<Vector2Int> PrioritiseGhostWall(List<Direction> allDirs)
    {
        Direction opposite = GetOppositeDirection(currentDirection);
        Direction? toGhostWall = GetDirFromCoordinates(ghostWallPos);

        allDirs = allDirs.OrderBy(d =>
        {
            if (toGhostWall.HasValue && d == toGhostWall.Value)
                return 0;

            if (d == currentDirection)
                return 1;

            if (d == opposite)
                return 3;

            return 2;
        }).ToList();
        
        List<Vector2Int> newCoords = new List<Vector2Int>();
        for (int i = 0; i < allDirs.Count; i++)
        {
            newCoords.Add(GetCoordinatesFromDir(allDirs[i]));
            Debug.Log(allDirs[i]);
        }
        return newCoords;
    }*/

    Vector2Int GetDirToGhostWall(List<Vector2Int> allDirs)
    {
        return GetNearestCoords(allDirs.ToArray(), ghostWallPos);
    }
    
    List<Vector2Int> PrioritiseWall(List<Direction> allDirs)
    {
        Direction opposite = GetOppositeDirection(currentDirection);
        Direction localSide = touchingOuterWall%2==0 ? LocalLeftDir() : LocalRightDir();
        
        allDirs = allDirs.OrderBy(d =>
        {
            if (d == localSide) return 0;
            if (d == currentDirection) return 1;
            if (d == opposite) return 3;
            return 2;
        }).ToList();
                
        List<Vector2Int> newCoords = new List<Vector2Int>();
        for (int i = 0; i < allDirs.Count; i++)
        {
            newCoords.Add(GetCoordinatesFromDir(allDirs[i]));
        }
        return newCoords;
    }

    List<Vector2Int> PrioritiseFrontCoords(List<Direction> allDirs)
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
        if (IsCoordinatesInsideBounds(newCoordinates))
        { 
            GridData data =  gridData[newCoordinates.x, newCoordinates.y];
            
            if (ghostManager.IsInsideGhostBox(transform.position))
                return data.walkable.Equals(GridData.Walkable.walkable) || data.walkable.Equals(GridData.Walkable.ghostWall);
            return data.walkable.Equals(GridData.Walkable.walkable);
        }
        return false;
    }

    bool IsCoordinatesInsideBounds(Vector2Int coords)
    {
        return (coords.x < gridData.GetLength(0) && coords.y < gridData.GetLength(1))
            &&  (coords.x >= 0 && coords.y >= 0);
    }

    Direction GetDirFromCoordinates(Vector2Int coordinates)
    {
        Vector2Int diff = coordinates - currentCoordinates;

        if (diff == Vector2Int.left)
            return Direction.up;
        if (diff == Vector2Int.right)
            return Direction.down;
        if (diff == Vector2Int.down)
            return Direction.left;
        if (diff == Vector2Int.up)
            return Direction.right;
        
        return Direction.up;
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

    public void OverrideBehaviour(Behaviour behaviour)
    {
        currentBehaviour = behaviour;
    }

    public void Die()
    {
        if (isDead) return;
        ghostManager.numGhostsDead++;
        StopAllCoroutines();
        StartCoroutine(DieCoroutine());
    }

    public void ResetGhost()
    {
        StopAllCoroutines();
        transform.position = gridData[startPosition.x, startPosition.y].position;
        currentDirection = defaultDirection;
        currentCoordinates  = startPosition;
        touchingOuterWall = 0;

        if (isDead)
            ghostManager.numGhostsDead--;
        isDead = false;
        isLerping = false;

        StartCoroutine(Move());
    }

    float GetDurationFlyToSpawn()
    {
        float distance = Vector3.Distance(transform.position, gridData[startPosition.x, startPosition.y].position);
        return  distance / speed;
    }
    
    IEnumerator DieCoroutine()
    {
        isDead = true;
        isLerping = false;
        animator.SetLayerWeight(3, 1);
        float duration = GetDurationFlyToSpawn();
        Vector2 startPos =  transform.position;
        Vector2 targetPos =  gridData[startPosition.x, startPosition.y].position;
        
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, targetPos, t/duration);
            yield return null;
        }

        transform.position = targetPos;
        currentDirection = defaultDirection;
        currentCoordinates  = startPosition;
        touchingOuterWall = 0;
        
        isDead = false;
        ghostManager.numGhostsDead--;
        
        animator.SetLayerWeight(3, 0);
        
        StartCoroutine(Move());
    }

    public void SetGhostWall(Transform ghostWall)
    {
        Vector2 wallPos = ghostWall.position;
        ghostWallPos = LevelGridManager.GetCoordinatesFromPoint(wallPos);
    }
}
