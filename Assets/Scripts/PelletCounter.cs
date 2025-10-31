using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletCounter : MonoBehaviour
{
    private void Start()
    {
        GameManager.instance.CountPellets(1);
    }
}
