using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerHandler : MonoBehaviour
{
    List<Transform> tracers = new List<Transform>();
    private float maxMouseDist = 0.2f;
    
    bool isTracing = false;
    
    MinigameManager miniGameManager;

    private int startChildCount = 0;
    private int traced = 0;

    public void SetMinigameManager(MinigameManager man)
    {
        miniGameManager = man;
    }

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            tracers.Add(transform.GetChild(i));
        }
        startChildCount = transform.childCount;
    }

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        if (Input.GetMouseButtonDown(0))
        {
            isTracing = true;
        }
        else if (Input.GetMouseButtonUp(0) && isTracing)
        {
            isTracing = false;
        }

        foreach (Transform tracer in tracers)
        {
            if (!tracer.gameObject.activeInHierarchy) continue;
            if (isTracing)
            {
                float dist = Vector3.Distance(mousePos, tracer.position);
                if (dist < maxMouseDist)
                {
                    // tracer is traced
                    miniGameManager.minigameScore += 1;
                    AudioManager.instance.PlayAudioRandom("btn");
                    Transform particle = tracer.GetChild(3);
                    particle.parent = null;
                    particle.gameObject.SetActive(true);
                    tracer.gameObject.SetActive(false);
                    tracer.gameObject.name = "Done";
                }
            }
        }

        if (AreAllChildrenInactive())
        {
            miniGameManager.GameCompleted = true;
            miniGameManager.minigameScore /= startChildCount;
            Destroy(gameObject);
            //Debug.Log("Finished TracerHandler");
        }
    }

    bool AreAllChildrenInactive()
    {
        foreach (Transform child in tracers)
        {
            if (child.gameObject.name != "Done") return false;
        }
        return true;
    }
}
