using System.Collections;
using UnityEngine;

public enum GameState
{
    Countdown,
    InGame,
    PacStudentDying,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Reference")]
    public UIManager uiManager;

    [Header("Audio Settings")]
    public AudioSource bgmAudioSource;
    public AudioClip introMusic;
    public AudioClip normalBackgroundMusic;

    [Header("Game State")]
    public GameState currentState { get; private set; } = GameState.Countdown;
    public int currentScore { get; private set; } = 0;
    public int totalPelletsRemaining { get; private set; } = 0;

    private float gameTimer = 0f;

    private const int ScoreNormalPellet = 10;
    private const int ScorePowerPellet = 50;
    private const int ScoreCherry = 100;
    private const int ScoreGhost = 300;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (uiManager == null)
        {
            Debug.LogWarning("GameManager: UIManager reference is missing in the Inspector!");
        }
        else
        {
            uiManager.UpdateScoreUI(currentScore);
            uiManager.UpdateGameTimerUI(0f);
        }

        StartCoroutine(StartCountdownRoutine());
    }

    private void Update()
    {
        if (currentState == GameState.InGame)
        {
            gameTimer += Time.deltaTime;
            if (uiManager != null)
            {
                uiManager.UpdateGameTimerUI(gameTimer);
            }
        }
    }

    private IEnumerator StartCountdownRoutine()
    {
        currentState = GameState.Countdown;

        if (bgmAudioSource != null && introMusic != null)
        {
            bgmAudioSource.clip = introMusic;
            bgmAudioSource.loop = false;
            bgmAudioSource.Play();
        }

        if (uiManager != null) uiManager.ShowCountdownText("3");
        yield return new WaitForSeconds(1f);

        if (uiManager != null) uiManager.ShowCountdownText("2");
        yield return new WaitForSeconds(1f);

        if (uiManager != null) uiManager.ShowCountdownText("1");
        yield return new WaitForSeconds(1f);

        if (uiManager != null) uiManager.ShowCountdownText("GO!");
        yield return new WaitForSeconds(1f);

        if (uiManager != null) uiManager.HideCountdownOverlay();

        currentState = GameState.InGame;

        if (bgmAudioSource != null && normalBackgroundMusic != null)
        {
            bgmAudioSource.clip = normalBackgroundMusic;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }

    public void SetTotalPellets(int count)
    {
        totalPelletsRemaining = count;
        Debug.Log("Total Pellets Registered: " + totalPelletsRemaining);
    }

    public void OnPelletEaten()
    {
        if (currentState != GameState.InGame) return;

        AddScore(ScoreNormalPellet);
        totalPelletsRemaining--;

        CheckLevelComplete();
    }

    public void OnPowerPelletEaten()
    {
        if (currentState != GameState.InGame) return;

        AddScore(ScorePowerPellet);
        totalPelletsRemaining--;

        TriggerScaredState();
        CheckLevelComplete();
    }

    public void OnCherryEaten()
    {
        if (currentState != GameState.InGame) return;

        AddScore(ScoreCherry);
    }

    public void OnGhostEaten()
    {
        if (currentState != GameState.InGame) return;

        AddScore(ScoreGhost);
    }

    public void OnGhostTouched(GameObject ghost)
    {
        if (currentState != GameState.InGame) return;

        Debug.Log("PacStudent touched a Ghost! Processing life loss...");
    }

    private void AddScore(int amount)
    {
        currentScore += amount;

        if (uiManager != null)
        {
            uiManager.UpdateScoreUI(currentScore);
        }
    }

    private void TriggerScaredState()
    {
        Debug.Log("Ghosts entered Frightened State!");
    }

    private void CheckLevelComplete()
    {
        if (totalPelletsRemaining <= 0)
        {
            currentState = GameState.GameOver;
            Debug.Log("Level Complete! All pellets consumed.");
        }
    }
}