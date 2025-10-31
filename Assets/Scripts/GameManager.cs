using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("In-Scene References")]
    public GhostsManager Ghosts;
    public CherryController Cherry;
    
    [HideInInspector]
    public bool gameReady = false;
    [HideInInspector]
    public UnityEvent OnGameReady;
    
    [Header("Game State")]
    public GameState state;
    public enum GameState
    {
        normal,
        scared,
        eaten
    }
    
    [SerializeField, Header("Score UI")]
    TextMeshProUGUI[] scoreTexts;
    private int _score = 0;
    int score
    {
        set
        {
            _score = value;
            foreach (TextMeshProUGUI scoreText in scoreTexts)
            {
                scoreText.text = _score.ToString("000000");
            }
        }
        get { return _score; }
    }

    [SerializeField, Header("Timer UI")]
    private TextMeshProUGUI[] timerTexts;
    private float timer = 0f;
    
    private float scaredTimer = 0f;
    [SerializeField, Header("Scared Timer UI")]
    private GameObject[] scaredTimerObjs;
    private TextMeshProUGUI[] scaredTimerTexts = new TextMeshProUGUI[2];
    
    [SerializeField, Header("Lives UI")]
    private Transform[] lifeUIParents;
    private int _lives = 3;
    int lives
    {
        get { return _lives; }
        set
        {
            _lives = value;
            foreach (Transform parent in lifeUIParents)
            {
                parent.GetChild(lives).gameObject.SetActive(false);
            }
        }
    }
    
    [SerializeField, Header("Go Timer UI")]
    private TextMeshProUGUI goTimerText;
    
    private void Awake()
    {
        instance = this;

        for (int i = 0; i < scaredTimerObjs.Length; i++)
        {
            scaredTimerTexts[i] = scaredTimerObjs[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        OnGameReady.AddListener(()=> Cherry.SpawnCherry());
    }
    
    void Start()
    {
        StartCoroutine(GoTimer());
        
    }

    private void Update()
    {
        if (!gameReady) return;
        
        UpdateTimer();
        if (state == GameState.scared)
        {
            UpdateScaredTimer();
        }
    }
    
    private void StartGame()
    {
        ActivateScaredMode(false);
    }

    public void AddScore(int scoreToAdd)
    {
        score += scoreToAdd;
    }

    
    private void UpdateTimer()
    {
        timer += Time.deltaTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
        string timeString =  string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds/10);
        foreach (TextMeshProUGUI timerText in timerTexts)
        {
            timerText.text = timeString;
        }
    }

    
    public void ActivateScaredMode(bool active)
    {
        state = active? GameState.scared :  GameState.normal;
        scaredTimer = active? 10f : 0;
        foreach (var scareObj in scaredTimerObjs)
        {
            scareObj.SetActive(active);
        }
        
        // ghost animator scared : normal
        Ghosts.ghostState = active? GhostsManager.GhostState.Scared : GhostsManager.GhostState.Normal;
        
        AudioManager.instance.StopAudio(active? "gameBGM" : "gameScared");
        AudioManager.instance.PlayAudioIncreasing(active? "gameScared" : "gameBGM", active? 0f: 0.5f);
    }

    void UpdateScaredTimer()
    {
        if (scaredTimer <= 0)
            ActivateScaredMode(false);

        if (scaredTimer <= 3f && Ghosts.ghostState == GhostsManager.GhostState.Scared)
        {
            // ghost animator recovering
            Ghosts.ghostState = GhostsManager.GhostState.Recovering;
        }
        
        scaredTimer -= Time.deltaTime;
        
        TimeSpan timeSpan = TimeSpan.FromSeconds(scaredTimer);
        string timeString = timeSpan.Seconds.ToString("00");
        foreach (TextMeshProUGUI scaredTimerText in scaredTimerTexts)
        {
            scaredTimerText.text = timeString;
        }
    }
    
    public bool CanHitGhost(GhostController ghost)   // add ghost hit as param
    {
        Debug.Log(Ghosts.ghostState);
        switch (Ghosts.ghostState)
        {
            case GhostsManager.GhostState.Normal:
                // lose a life
                lives--;
                return false;   // play die anim, particles, sound
            case GhostsManager.GhostState.Scared or GhostsManager.GhostState.Recovering:
                // ghost die > animator dead
                ghost.Die();
                AddScore(300);
                return true;
        }

        return false;
    }


    IEnumerator GoTimer()
    {
        AudioManager.instance.PlayAudio("intro");
        for (int i = 3; i >= 0; i--)
        {
            goTimerText.text = i==0? "Go!" : i.ToString();
            yield return new WaitForSeconds(0.7f);
        }

        gameReady = true;
        goTimerText.transform.parent.gameObject.SetActive(false);
        StartGame();
    }
}
