using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public float speed;
    GridData[,] gridData;
    private Vector2Int currentCoordinates;
    enum PlayerInput
    {
        none,
        w,
        a,
        s,
        d
    };

    PlayerInput lastInput = PlayerInput.none; // last move dir
    PlayerInput currentInput = PlayerInput.none; // curr move dir

    private bool isLerping;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        gridData = LevelGridManager.gridData;
        currentCoordinates = LevelGridManager.GetCoordinatesFromPoint(transform.position);

        StartCoroutine(ApplyPlayerInput());
    }
    
    IEnumerator ApplyPlayerInput()
    {
        while (true)
        {
            if (!isLerping)
            {
                Vector2Int newCoordinates;
                if (IsWalkable(currentCoordinates, out newCoordinates))
                {
                    ApplyMoveAnimation(newCoordinates - currentCoordinates);
                    yield return LerpPlayer(newCoordinates);
                }
            }
            
            yield return null;
        }
    }
    

    bool IsWalkable(Vector2Int prevCoordinates, out Vector2Int newCoordinates)
    {
        Vector2Int testCoordinates;
        
        if (IsInputValid(lastInput, prevCoordinates, out testCoordinates))
        {
            currentInput = lastInput;
            newCoordinates = testCoordinates;
            return true;
        }
        
        return IsInputValid(currentInput, prevCoordinates, out newCoordinates);
    }

    bool IsInputValid(PlayerInput input, Vector2Int prevCoordinates, out Vector2Int newCoordinates)
    {
        newCoordinates = prevCoordinates;
        switch (input)
        {
            case PlayerInput.none:
                break;
            case PlayerInput.w:
                newCoordinates.x -= 1;
                break;
            case PlayerInput.a:
                newCoordinates.y -= 1;
                break;
            case PlayerInput.s:
                newCoordinates.x += 1;
                break;
            case PlayerInput.d:
                newCoordinates.y += 1;
                break;
        }
        
        Debug.Log(input.ToString() + " " + gridData[newCoordinates.x, newCoordinates.y].walkable.Equals(GridData.Walkable.walkable));
        return gridData[newCoordinates.x, newCoordinates.y].walkable.Equals(GridData.Walkable.walkable);
    }

    IEnumerator LerpPlayer(Vector2Int targetCoordinates)
    {
        isLerping = true;
        float t = 0;
        float duration = speed / 1; //1 tile

        Vector2 startPos = gridData[currentCoordinates.x, currentCoordinates.y].position;
        Vector2 endPos = gridData[targetCoordinates.x, targetCoordinates.y].position;
        
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        currentCoordinates = targetCoordinates;
        isLerping = false;
    }

    void ApplyMoveAnimation(Vector2 dir)
    {
    }

    private void Update()
    {
        UpdatePlayerInput();
    }

    void UpdatePlayerInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            lastInput = PlayerInput.w;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            lastInput = PlayerInput.a;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            lastInput = PlayerInput.s;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            lastInput = PlayerInput.d;
        }
    }
}
