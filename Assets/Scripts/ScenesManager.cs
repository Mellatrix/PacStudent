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
        SceneManager.sceneLoaded += InitialiseLevel;
        InitialiseLevel();
    }

    private void PlayBGM(Scene scene)
    {
        switch (scene.buildIndex)
        {
            case 0:
                AudioManager.instance.PlayAudio("startBGM");
                break;
            /*case >0:
                AudioManager.instance.PlayAudio("gameBGM");
                break;*/
            default:
                break;
        }
    }

    private void InitialiseLevel(Scene scene = default, LoadSceneMode mode = LoadSceneMode.Single)
    {
        SetSceneButtons();
        
        // PlayBGM(SceneManager.GetActiveScene());
    }

    private void SetSceneButtons()
    {
        GameObject.FindGameObjectWithTag("L1")?.GetComponent<Button>().
            onClick.AddListener(() => LoadScene("Level1"));
        GameObject.FindGameObjectWithTag("L2")?.GetComponent<Button>().
            onClick.AddListener(() => LoadScene("Level2"));
        GameObject.FindGameObjectWithTag("Exit")?.GetComponent<Button>().
            onClick.AddListener(() => LoadScene("StartScene"));
    }
    
    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
        Debug.Log(scene);
    }
}
