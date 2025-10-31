using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public Direction defaultDirection;
    Animator animator;
    GhostsManager ghostManager;
    
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

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentDirection = defaultDirection;
    }

    public void Move()
    {
        if (!GameManager.instance.gameReady) return;
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
