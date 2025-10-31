using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostsManager : MonoBehaviour
{
    private Animator[] animators;
    private GhostController[]  ghosts;
    //public bool canMove = false;
    [SerializeField]
    private Transform[] ghostWalls;

    private PacStudentController player;
    
    public enum GhostState
    {
        Normal,
        Scared,
        Recovering
    };
    
    GhostState state = GhostState.Normal;

    public GhostState ghostState {
        get { return state; }
        set
        {
            state = value; 
            //Debug.Log(state);
            UpdateGhosts();
        }
    }

    private Bounds ghostBox;

    private void Awake()
    {
        ghostBox = GetComponent<Collider2D>().bounds;
        animators = GetComponentsInChildren<Animator>(true);
        ghosts = GetComponentsInChildren<GhostController>(true);
    }

    private void Start()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].SetGhostManager(this);
            ghosts[i].SetGhostWall(ghostWalls[i]);
        }

        player = GameManager.instance.Player;
    }

    public bool IsInsideGhostBox(Vector2 pos)
    {
        return ghostBox.Contains(pos);
    }

    void UpdateGhosts()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].speed = ghostState == GhostState.Normal ? player.speed * 0.9f : 0.5f;
            
            /*animator.SetInteger("GhostState", (int)ghostState);*/
            for (int j = 0; j < 3; j++)
            {
                animators[j].SetLayerWeight(i, (int)state == j? 1 : 0);
                
            }
        }
    }

}
