using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    public GameObject sixteenNine, fourThree;

    private void Update()
    {
        float aspect =  (float)Screen.width / (float)Screen.height;
        fourThree.SetActive(aspect < 1.5f);
        sixteenNine.SetActive(aspect >= 1.5f);
    }
}
