using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostsManager : MonoBehaviour
{
    private Animator[] animators;
    private GhostController[]  ghosts;
    //public bool canMove = false;
    
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
            Debug.Log(state);
            UpdateAnimators();
        }
    }

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
        ghosts = GetComponentsInChildren<GhostController>();
        foreach (GhostController ghost in ghosts)
        {
            ghost.SetGhostManager(this);
        }
    }

    void UpdateAnimators()
    {
        foreach (Animator animator in animators)
        {
            /*animator.SetInteger("GhostState", (int)ghostState);*/
            for (int i = 0; i < 3; i++)
            {
                animator.SetLayerWeight(i, (int)state == i? 1 : 0);
            }
        }
    }

}
