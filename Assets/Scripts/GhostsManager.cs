using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostsManager : MonoBehaviour
{
    private Animator[] animators;
    
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
            UpdateAnimators();
        }
    }

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
    }

    void UpdateAnimators()
    {
        foreach (Animator animator in animators)
        {
            Debug.Log(ghostState.ToString());
            animator.SetTrigger(ghostState.ToString());
        }
    }

}
