using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GoblinMovementManager : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;
    Vector3 startPoint;
    int wayPointIndex = 0;
    
    [SerializeField] float speed;
    private float duration;
    private float timeElapsed = 0f;
    
    private Animator animator;
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] moveSounds;
    private bool isMoving = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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

        duration = GetDuration();

        StartCoroutine(PlayMoveSounds());
    }

    float GetDuration() // for a constant speed
    {
        float distance = Vector3.Distance(startPoint, waypoints[wayPointIndex].position);
        return  distance / speed;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, waypoints[wayPointIndex].position) > 0.05f)
        {
            isMoving = true;
            transform.position = Vector3.Lerp(startPoint, waypoints[wayPointIndex].position, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
        }
        else
        {
            transform.position = waypoints[wayPointIndex].position;
            startPoint = waypoints[wayPointIndex].position;
            timeElapsed = 0;
            wayPointIndex = (wayPointIndex + 1) % waypoints.Length;
            duration = GetDuration();
            UpdateAnimator();
        }
    }

    IEnumerator PlayMoveSounds()
    {
        while (true)
        {
            if (isMoving)
            {
                audioSource.PlayOneShot(GetRandomMoveSound());
                yield return new WaitForSeconds(speed/3f);
            }
            else
            {
                yield return null;
            }
        }
    }

    void UpdateAnimator()
    {
        Vector3 direction = (waypoints[wayPointIndex].position - startPoint).normalized;
        if (direction == Vector3.up)
            animator.SetTrigger("Up");
        else if (direction == Vector3.down)
            animator.SetTrigger("Down");
        else if (direction == Vector3.left)
            animator.SetTrigger("Left");
        else if (direction == Vector3.right)
            animator.SetTrigger("Right");
    }

    AudioClip GetRandomMoveSound()
    {
        return moveSounds[Random.Range(0, moveSounds.Length)];
    }
}
