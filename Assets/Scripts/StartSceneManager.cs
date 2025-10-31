using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI highScore1, time1, highScore2, time2;
    
    private void Start()
    {
        AudioManager.instance.PlayAudio("startBGM");
        InitialiseHighScores();
    }

    void InitialiseHighScores()
    {
        highScore1.text = "High Score: " + PlayerPrefs.GetInt("Level1HS", 0);
        highScore2.text = "High Score: " + PlayerPrefs.GetInt("Level2HS", 0);
        time1.text = "Time: " + GetTimeString(PlayerPrefs.GetFloat("Level1Time"));
        time2.text = "Time: " + GetTimeString(PlayerPrefs.GetFloat("Level2Time"));
    }

    string GetTimeString(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string timeString =  string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds/10);
        return timeString;
    }
}
