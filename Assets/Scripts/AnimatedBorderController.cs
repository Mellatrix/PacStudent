using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimatedBorderController : MonoBehaviour
{
    public float speed = 1;
    private TextMeshProUGUI[] textBorders;
    public string charsString;
    private string[] chars;

    private void Awake()
    {
        textBorders = GetComponentsInChildren<TextMeshProUGUI>();
        
        chars = charsString.Split(' ');
    }

    private void Start()
    {
        StartCoroutine(AnimateBorderText());
    }

    IEnumerator AnimateBorderText()
    {
        string newString;
        List<string> auxList;
        int index;
        
        while (true)
        {
            yield return new WaitForSeconds(1/speed);
            foreach (TextMeshProUGUI textBorder in textBorders)
            {
                auxList = new List<string>(chars);
                newString = "";
                
                for (int i = 0; i < chars.Length; i++)
                {
                    index = Random.Range(0, auxList.Count);
                    newString += auxList[index];
                    auxList.RemoveAt(index);
                    if (i < chars.Length - 1)
                        newString += " ";
                }
                
                textBorder.text = newString;
            }
        }
    }
}
