using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public Direction defaultDirection;
    Animator animator;
    
    public enum Direction
    {
        up,
        down,
        left,
        right
    };

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

    public void Die()
    {
        animator.SetBool("Die", true);
    }
}
