using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    private Vector3 offset;
    public float smoothSpeed = 1.5f;

    public Bounds fourthreeBounds, sixteenNineBounds;
    private Camera camera;

    private float camHeight, camWidth;
    
    private void Awake()
    {
        camera = GetComponent<Camera>();
        offset = transform.position;
    }

    private void Start()
    {
        camHeight = camera.orthographicSize;
        camWidth = camera.aspect * camera.orthographicSize;
    }

    private void LateUpdate()
    {
        SmoothFollowTarget();
    }

    void SmoothFollowTarget()
    {
        if (target == null) return;
        Vector3 targetPos = target.position + offset;
        Vector3 smoothPos = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        
        transform.position = new Vector3(GetXClamp(smoothPos), GetYClamp(smoothPos), transform.position.z);
    }

    float GetXClamp(Vector3 smoothPos)
    {
        float aspect =  (float)Screen.width / (float)Screen.height;
        Bounds camBounds = aspect < 1.5f? fourthreeBounds : sixteenNineBounds;
        return Mathf.Clamp(smoothPos.x, camBounds.min.x + camWidth, camBounds.max.x - camWidth);
    }

    float GetYClamp(Vector3 smoothPos)
    {
        float aspect =  (float)Screen.width / (float)Screen.height;
        Bounds camBounds = aspect < 1.5f? fourthreeBounds : sixteenNineBounds;
        return Mathf.Clamp(smoothPos.y, camBounds.min.y + camWidth, camBounds.max.y - camHeight);
    }
}
