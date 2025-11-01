using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("In-Scene References")]
    public GhostsManager Ghosts;
    public CherryController Cherry;
    public PacStudentController Player;
    
    [HideInInspector]
    public bool gameReady = false;
    bool gameOver = false;
    [HideInInspector]
    public UnityEvent OnGameReady;
    
    [Header("Game State")]
    public GameState state;
    public enum GameState
    {
        normal,
        scared,
        recovering
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
    //private bool ghostEaten = false;
    
    [SerializeField, Header("Lives UI")]
    private Transform[] lifeUIParents;
    private int _lives = 3;
    int lives
    {
        get { return _lives; }
        set
        {
            _lives = value;
            if (lives < 0) return;
            foreach (Transform parent in lifeUIParents)
            {
                parent.GetChild(lives).gameObject.SetActive(false);
            }
        }
    }
    
    [SerializeField, Header("Go Timer UI")]
    private TextMeshProUGUI goTimerText;

    [SerializeField, Header("Game Over UI")]
    private GameObject gameOverPanel;

    private int pelletCount;
    
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

    public void CountPellets(int num)
    {
        pelletCount += num;
        if (pelletCount <= 0)
            EndGame(true);
    }

    private void Update()
    {
        if (!gameReady || gameOver) return;
        
        UpdateTimer();
        if (state == GameState.scared)
        {
            UpdateScaredTimer();
        }
    }
    
    private void StartGame()
    {
        ActivateScaredMode(false);
        OnGameReady.Invoke();
    }

    private void EndGame(bool win)
    {
        gameOver = true;
        gameOverPanel.SetActive(true);
        AudioManager.instance.DestroyLevelMusic();
        AudioManager.instance.PlayAudio(win? "win" : "loss");
        if (!win)
            AudioManager.instance.PlayAudioRandom("losecry");
        int highScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name+"HS", 0);
        float bestTime = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name+"Time", 0);
        if (score > highScore || (highScore == score && timer < bestTime))
        {
            PlayerPrefs.SetInt(SceneManager.GetActiveScene().name+"HS", score);
            PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name+"Time", timer);
        }
        
        PlayerPrefs.Save();
        
        Invoke("ReturnToStartScene", 3f);
    }

    void ReturnToStartScene()
    {
        ScenesManager.instance.LoadScene("StartScene");
    }

    public void AddScore(int scoreToAdd)
    {
        score += scoreToAdd;
    }
    
    private void UpdateTimer()
    {
        timer += Time.deltaTime;
        
        string timeString = GetTimeString();
        foreach (TextMeshProUGUI timerText in timerTexts)
        {
            timerText.text = timeString;
        }
    }

    string GetTimeString()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
        string timeString =  string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds/10);
        return timeString;
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
                if (lives <= 0)
                    EndGame(false);
                
                // ghosts should not move, reset ghosts to initial position
                StartCoroutine(WaitForPlayerRespawn());
                return false;   // play die anim, particles, sound, respawn player
            case GhostsManager.GhostState.Scared or GhostsManager.GhostState.Recovering:
                // ghost die > animator dead
                ghost.Die();
                AudioManager.instance.PlayAudioRandom("eatGhost");
                AudioManager.instance.PlayAudioRandom("throw");
                
                AddScore(300);
                return true;
        }

        return false;
    }

    IEnumerator WaitForPlayerRespawn()
    {
        gameReady = false;
        yield return new WaitForSeconds(1f);    // wait for player to finish die anim, particles, sound, respawn
        if (gameOver) yield break;
        Ghosts.ResetAllGhosts();
        gameReady = true;
        
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
