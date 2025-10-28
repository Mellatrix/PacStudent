using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager instance;
    
    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => AudioManager.instance.PlayAudio("btn"));
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += PlayBGM;
        PlayBGM(SceneManager.GetActiveScene());
    }

    private void PlayBGM(Scene scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        switch (scene.buildIndex)
        {
            case 0:
                AudioManager.instance.PlayAudio("startBGM");
                break;
            case >0:
                AudioManager.instance.PlayAudio("gameBGM");
                break;
            default:
                break;
        }
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
