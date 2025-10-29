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
    public GhostsManager Ghosts;
    public CherryController Cherry;
    
    public bool gameReady = false;
    
    public UnityEvent OnGameReady;

    public enum GameState
    {
        normal,
        scared,
        eaten
    }
    public GameState state;

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < scaredTimerObjs.Length; i++)
        {
            scaredTimerTexts[i] = scaredTimerObjs[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        OnGameReady.AddListener(()=> Cherry.SpawnCherry());
    }

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
    [SerializeField]
    TextMeshProUGUI[] scoreTexts;

    public void AddScore(int scoreToAdd)
    {
        score += scoreToAdd;
    }

    [SerializeField]
    private TextMeshProUGUI[] timerTexts;
    private float timer = 0f;
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

    private float scaredTimer = 0f;
    [SerializeField]
    private GameObject[] scaredTimerObjs;
    private TextMeshProUGUI[] scaredTimerTexts = new TextMeshProUGUI[2];
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
        AudioManager.instance.PlayAudio(active? "gameScared" : "gameBGM");
    }

    void UpdateScaredTimer()
    {
        if (scaredTimer <= 0)
            ActivateScaredMode(false);

        if (scaredTimer <= 3f)
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

    void Start()
    {
        StartCoroutine(GoTimer());
        
    }

    private void StartGame()
    {
        ActivateScaredMode(false);
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

    [SerializeField]
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

    public bool CanHitGhost()
    {
        switch (Ghosts.ghostState)
        {
            case GhostsManager.GhostState.Normal:
                // lose a life
                lives--;
                return false;   // play die anim, particles, sound
            case GhostsManager.GhostState.Scared or GhostsManager.GhostState.Recovering:
                // ghost die > animator dead
                AddScore(300);
                return true;
        }

        return false;
    }

    public TextMeshProUGUI goTimerText;

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
