using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager instance;
    public UnityAction OnSceneLoaded, BeforeSceneLoaded;
    
    private void Awake()
    {
        if (instance == null && instance != this)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => AudioManager.instance.PlayAudio("btn"));
        }

    }

    void Start()
    {
        OnSceneLoaded += InitialiseLevel;
        OnSceneLoaded.Invoke();
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

    private void InitialiseLevel()
    {
        SetSceneButtons();
        //Debug.Log("InitialiseLevel");
        // PlayBGM(SceneManager.GetActiveScene());
    }

    private void SetSceneButtons()
    {
        GameObject.FindGameObjectWithTag("L1")?.GetComponent<Button>().
            onClick.AddListener(() => LoadScene("Level1"));
        GameObject.FindGameObjectWithTag("L2")?.GetComponent<Button>().
            onClick.AddListener(() => LoadScene("InnovationScene"));
        GameObject.FindGameObjectWithTag("Exit")?.GetComponent<Button>().
            onClick.AddListener(() => LoadScene("StartScene"));
    }
    
    public void LoadScene(string scene)
    {
        StartCoroutine(LoadSceneRoutine(scene));
    }

    IEnumerator LoadSceneRoutine(string scene)
    {
        BeforeSceneLoaded.Invoke();
        Debug.Log("LoadSceneRoutine");
        yield return SceneManager.LoadSceneAsync(scene);
        OnSceneLoaded.Invoke();
    }

    public int GetLevelIndex()
    {
        return
            SceneManager.GetActiveScene().buildIndex;
    }
}
