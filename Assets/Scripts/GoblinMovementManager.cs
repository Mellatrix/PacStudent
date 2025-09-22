using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinMovementManager : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    private AudioClip[] moveSounds;
    [SerializeField]
    Transform[] waypoints;
    Vector3 startPoint;
    int wayPointIndex = 0;
    [SerializeField]
    float duration;
    private float timeElapsed = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        startPoint = transform.position;
        
        float maxDist = 30f;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (Vector3.Distance(transform.position, waypoints[i].position) < maxDist)
            {
                maxDist = Vector3.Distance(transform.position, waypoints[i].position);
                wayPointIndex = i;
            }
        }
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, waypoints[wayPointIndex].position) > 0.05f)
        {
            transform.position = Vector3.Lerp(startPoint, waypoints[wayPointIndex].position, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
        }
        else
        {
            transform.position = waypoints[wayPointIndex].position;
            startPoint = waypoints[wayPointIndex].position;
            timeElapsed = 0;
            wayPointIndex = (wayPointIndex + 1) % waypoints.Length;
        }
    }

    
}
