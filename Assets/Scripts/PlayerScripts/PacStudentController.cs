using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// using this mainly for movement and animation
/// </summary>
public class PacStudentController : MonoBehaviour
{
    public float speed;
    GridData[,] gridData;
    private Vector2Int currentCoordinates;

    public Vector2Int GetCoordinates()
    {
        return currentCoordinates;
    }
    
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
    
    ParticleSystemRenderer particleRenderer;
    ParticleSystem particle;
    PlayerCollisionController collisionController;

    [SerializeField]
    private Transform[] teleporters;
    
    Vector2Int startPosition;

    public GameObject deathParticles;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        particleRenderer = GetComponentInChildren<ParticleSystemRenderer>();
        particle = GetComponentInChildren<ParticleSystem>();
        collisionController = GetComponent<PlayerCollisionController>();
        collisionController.SetPlayerController(this);
    }

    void Start()
    {
        gridData = LevelGridManager.gridData;
        currentCoordinates = LevelGridManager.GetCoordinatesFromPoint(transform.position);
        startPosition = currentCoordinates;
        
        StartCoroutine(ApplyPlayerInput());
    }
    
    IEnumerator ApplyPlayerInput()
    {
        currentCoordinates = LevelGridManager.GetCoordinatesFromPoint(transform.position);
        
        while (true)
        {
            if (GameManager.instance.gameReady)
            {
                if (!isLerping)
                {
                    Vector2Int newCoordinates;
                    if (IsWalkable(currentCoordinates, out newCoordinates))
                    {
                        ApplyMoveAnimation(gridData[newCoordinates.x, newCoordinates.y].position-gridData[currentCoordinates.x, currentCoordinates.y].position);
                        yield return LerpPlayer(newCoordinates);
                    }
                    else
                    {
                        animator.SetBool("Exit", true);
                        collisionController.OffsetCollider(Vector2.zero);
                    }
                }
                
                yield return null;
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
                return false;
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
        
        //Debug.Log(input.ToString() + " " + gridData[newCoordinates.x, newCoordinates.y].walkable.Equals(GridData.Walkable.walkable));
        //Debug.Log(newCoordinates);
        return gridData[newCoordinates.x, newCoordinates.y].walkable.Equals(GridData.Walkable.walkable);
    }

    IEnumerator LerpPlayer(Vector2Int targetCoordinates)
    {
        isLerping = true;
        float t = 0;
        float duration = 1/speed; //1 tile

        float stepSoundTimer = duration;

        Vector2 startPos = gridData[currentCoordinates.x, currentCoordinates.y].position;
        Vector2 endPos = gridData[targetCoordinates.x, targetCoordinates.y].position;
        
        while (t < duration)
        {
            if (stepSoundTimer >= duration/3)
            {
                stepSoundTimer = 0f;
                AudioManager.instance.PlayAudioRandom("walk");
            }
            t += Time.deltaTime;
            stepSoundTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t/duration);
            yield return null;
        }

        currentCoordinates = targetCoordinates;
        isLerping = false;
    }

    void ApplyMoveAnimation(Vector2 dir)
    {
        animator.SetBool("Exit", false);
        if (dir.normalized == Vector2.down)
        {
            animator.SetInteger("Direction", 1);
            collisionController.OffsetCollider(Vector2.down);
            particleRenderer.sortingOrder = 0;
            return;
        }

        if (dir.normalized == Vector2.up)
        {
            animator.SetInteger("Direction", 0);
            collisionController.OffsetCollider(Vector2.up);
        }
        else if (dir.normalized == Vector2.left)
        {
            animator.SetInteger("Direction", 2);
            collisionController.OffsetCollider(Vector2.left);
        }
        else if (dir.normalized == Vector2.right)
        {
            animator.SetInteger("Direction", 3);
            collisionController.OffsetCollider(Vector2.right);
        }

        particleRenderer.sortingOrder = 12;
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

    public void Teleport(Transform teleporter)
    {
        Vector2Int spawnPoint = teleporters[0] == teleporter ? new Vector2Int(14,27) : new Vector2Int(14,0);

        foreach (Transform tp in teleporters)
        {
            tp.gameObject.SetActive(false);
        }
        
        StopAllCoroutines();
        isLerping = false;
        
        SetPlayerPositionOnGrid(spawnPoint);

        StartCoroutine(ApplyPlayerInput());
        
        Invoke("EnableTeleporter", 0.7f);
    }

    void SetPlayerPositionOnGrid(Vector2Int newPosition)
    {
        particle.Stop();
        transform.position = gridData[newPosition.x, newPosition.y].position;
        currentCoordinates = newPosition;
        particle.Play();
    }

    void EnableTeleporter()
    {
        foreach (Transform tp in teleporters)
        {
            tp.gameObject.SetActive(true);
        }
    }

    public void Die()
    {
        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        animator.SetTrigger("Die");
        currentInput = PlayerInput.none;
        lastInput = PlayerInput.none;
        // play particles
        Instantiate(deathParticles, transform.position, Quaternion.identity);   
        yield return new WaitForSeconds(0.9f);
        
        SetPlayerPositionOnGrid(startPosition);
        animator.SetBool("Exit", true);
    }
}
