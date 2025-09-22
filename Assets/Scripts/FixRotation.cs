using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{
    private int quad;
    void Start()
    {
        Transform current = transform.parent;

        while (current != null)
        {
            if (current.name.Contains("Quadrant")) 
            {
                Vector3 targetScale = new Vector3(Mathf.Sign(current.localScale.x), Mathf.Sign(current.localScale.y), 0);
                transform.localScale = targetScale;
            }
            current = current.parent;
        }
    }
}
