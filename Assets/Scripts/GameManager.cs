using System;
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

    private const int StartingLives = 3;

    public const string HighScoreKey = "HighScore";
    public const string HighScoreTimeKey = "HighScoreTime";

    [Header("UI Reference")]
    public UIManager uiManager;

    [Header("Tweener Reference")]
    public Tweener tweener;

    [Header("PacStudent & Death References")]
    public PacStudentController pacStudent;
    public ParticleSystem deathParticle;
    public AudioSource sfxAudioSource;
    public AudioClip deathSound;
    public float deathAnimationDuration = 1.5f;

    [Header("Ghosts References")]
    public GhostController[] ghosts;
    private Vector3[] ghostStartPositions;

    [Header("Ghost Dead Settings")]
    public AudioClip ghostDeadSound;
    public float ghostDeadDuration = 3f;
    private int deadGhostCount = 0;
    private Coroutine[] deadRoutines;

    [Header("Collectible Sounds")]
    public AudioClip cherrySound;

    [Header("Audio Settings")]
    public AudioSource bgmAudioSource;
    public AudioClip introMusic;
    public AudioClip normalBackgroundMusic;
    public AudioClip scaredMusic;
    public AudioClip deadMusic;

    [Header("Ghost Scared Settings")]
    [SerializeField] private float scaredDuration = 10f;
    [SerializeField] private float recoverTime = 3f;

    [Header("Game State")]
    public GameState currentState { get; private set; } = GameState.Countdown;
    public int currentScore { get; private set; } = 0;
    public int totalPelletsRemaining { get; private set; } = 0;
    public int lives { get; private set; } = StartingLives;

    private float gameTimer = 0f;
    private float scaredRemaining = 0f;
    private Coroutine scaredRoutine;
    private bool gameEnded = false;

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
        lives = StartingLives;

        if (pacStudent == null)
        {
            Debug.LogWarning("GameManager: pacStudent reference is missing in the Inspector!");
        }

        if (ghosts == null || ghosts.Length == 0)
        {
            Debug.LogWarning("GameManager: ghosts array is missing or empty in the Inspector!");
        }
        else
        {
            if (ghosts.Length < 4)
            {
                Debug.LogWarning("GameManager: ghosts array has less than 4 ghosts!");
            }

            ghostStartPositions = new Vector3[ghosts.Length];
            deadRoutines = new Coroutine[ghosts.Length];

            for (int i = 0; i < ghosts.Length; i++)
            {
                if (ghosts[i] != null)
                {
                    ghostStartPositions[i] = ghosts[i].transform.position;
                }
            }
        }

        if (uiManager == null)
        {
            Debug.LogWarning("GameManager: UIManager reference is missing in the Inspector!");
        }
        else
        {
            uiManager.UpdateScoreUI(currentScore);
            uiManager.UpdateGameTimerUI(0f);
            uiManager.UpdateLivesUI(lives);
            uiManager.HideScaredTimer();
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

        UpdateBgm();
    }

    public void UpdateBgm()
    {
        if (deadGhostCount > 0)
        {
            PlayBgm(deadMusic);
        }
        else if (scaredRoutine != null)
        {
            PlayBgm(scaredMusic);
        }
        else
        {
            PlayBgm(normalBackgroundMusic);
        }
    }

    private void PlayBgm(AudioClip clip)
    {
        if (bgmAudioSource == null || clip == null) return;

        if (bgmAudioSource.clip != clip || !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.clip = clip;
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

        if (sfxAudioSource != null && cherrySound != null)
        {
            sfxAudioSource.PlayOneShot(cherrySound);
        }
    }

    public void OnGhostEaten()
    {
        if (currentState != GameState.InGame) return;

        AddScore(ScoreGhost);
    }

    public void OnGhostTouched(GameObject ghost)
    {
        if (currentState != GameState.InGame) return;

        GhostController gc = ghost.GetComponentInParent<GhostController>();
        if (gc == null) return;

        switch (gc.CurrentState)
        {
            case GhostState.Normal:
                StartCoroutine(PacStudentDeathSequence());
                break;

            case GhostState.Scared:
            case GhostState.Recovering:
                EatGhost(gc);
                break;

            case GhostState.Dead:
                break;
        }
    }

    private void EatGhost(GhostController gc)
    {
        if (ghosts == null) return;

        int index = Array.IndexOf(ghosts, gc);
        if (index < 0) return;

        OnGhostEaten();
        gc.SetState(GhostState.Dead);
        deadGhostCount++;

        if (sfxAudioSource != null && ghostDeadSound != null)
        {
            sfxAudioSource.PlayOneShot(ghostDeadSound);
        }

        UpdateBgm();

        if (deadRoutines != null && index < deadRoutines.Length)
        {
            if (deadRoutines[index] != null)
            {
                StopCoroutine(deadRoutines[index]);
            }
            deadRoutines[index] = StartCoroutine(GhostDeadRoutine(gc, index));
        }
    }

    private IEnumerator GhostDeadRoutine(GhostController gc, int index)
    {
        yield return new WaitForSeconds(ghostDeadDuration);

        deadGhostCount--;

        if (scaredRoutine != null)
        {
            if (scaredRemaining <= recoverTime)
            {
                gc.SetState(GhostState.Recovering);
            }
            else
            {
                gc.SetState(GhostState.Scared);
            }
        }
        else
        {
            gc.SetState(GhostState.Normal);
        }

        if (deadRoutines != null && index < deadRoutines.Length)
        {
            deadRoutines[index] = null;
        }

        UpdateBgm();
    }

    private void TriggerScaredState()
    {
        if (scaredRoutine != null)
        {
            StopCoroutine(scaredRoutine);
        }

        scaredRoutine = StartCoroutine(ScaredRoutine());
    }

    private IEnumerator ScaredRoutine()
    {
        if (ghosts != null)
        {
            foreach (var ghost in ghosts)
            {
                if (ghost != null && ghost.CurrentState != GhostState.Dead)
                {
                    ghost.SetState(GhostState.Scared);
                }
            }
        }

        UpdateBgm();

        scaredRemaining = scaredDuration;
        bool recovering = false;

        while (scaredRemaining > 0)
        {
            if (uiManager != null)
            {
                uiManager.ShowScaredTimer(scaredRemaining);
            }

            if (scaredRemaining <= recoverTime && !recovering)
            {
                if (ghosts != null)
                {
                    foreach (var ghost in ghosts)
                    {
                        if (ghost != null && ghost.CurrentState != GhostState.Dead)
                        {
                            ghost.SetState(GhostState.Recovering);
                        }
                    }
                }
                recovering = true;
            }

            scaredRemaining -= Time.deltaTime;
            yield return null;
        }

        if (ghosts != null)
        {
            foreach (var ghost in ghosts)
            {
                if (ghost != null && ghost.CurrentState != GhostState.Dead)
                {
                    ghost.SetState(GhostState.Normal);
                }
            }
        }

        if (uiManager != null)
        {
            uiManager.HideScaredTimer();
        }

        scaredRoutine = null;
        UpdateBgm();
    }

    private IEnumerator PacStudentDeathSequence()
    {
        currentState = GameState.PacStudentDying;

        if (scaredRoutine != null)
        {
            StopCoroutine(scaredRoutine);
            scaredRoutine = null;
        }

        if (deadRoutines != null)
        {
            for (int i = 0; i < deadRoutines.Length; i++)
            {
                if (deadRoutines[i] != null)
                {
                    StopCoroutine(deadRoutines[i]);
                    deadRoutines[i] = null;
                }
            }
        }

        deadGhostCount = 0;

        if (uiManager != null)
        {
            uiManager.HideScaredTimer();
        }

        UpdateBgm();

        lives--;
        if (uiManager != null)
        {
            uiManager.UpdateLivesUI(lives);
        }

        if (pacStudent != null)
        {
            pacStudent.Die();

            if (deathParticle != null)
            {
                deathParticle.transform.position = pacStudent.transform.position;
                deathParticle.Play();
            }
        }

        if (sfxAudioSource != null && deathSound != null)
        {
            sfxAudioSource.PlayOneShot(deathSound);
        }

        yield return new WaitForSeconds(deathAnimationDuration);

        if (lives <= 0)
        {
            EndGame();
        }
        else
        {
            if (pacStudent != null) pacStudent.ResetToStart();

            ResetGhostsToStart();

            currentState = GameState.InGame;
        }
    }

    public void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        currentState = GameState.GameOver;

        if (scaredRoutine != null)
        {
            StopCoroutine(scaredRoutine);
            scaredRoutine = null;
        }

        if (deadRoutines != null)
        {
            for (int i = 0; i < deadRoutines.Length; i++)
            {
                if (deadRoutines[i] != null)
                {
                    StopCoroutine(deadRoutines[i]);
                    deadRoutines[i] = null;
                }
            }
        }

        if (uiManager != null)
        {
            uiManager.HideScaredTimer();
            uiManager.ShowCountdownText("GAME OVER");
            uiManager.SetButtonsInteractable(false);
        }

        if (tweener != null && pacStudent != null)
        {
            tweener.RemoveTween(pacStudent.transform);
        }

        if (tweener != null)
        {
            GameObject[] cherries = GameObject.FindGameObjectsWithTag("Cherry");
            foreach (GameObject cherry in cherries)
            {
                if (cherry != null)
                {
                    tweener.RemoveTween(cherry.transform);
                }
            }
        }

        SaveHighScore();

        yield return new WaitForSeconds(3f);

        if (uiManager != null)
        {
            uiManager.GoToStartScene();
        }
    }

    private void SaveHighScore()
    {
        bool isNewRecord = false;

        if (!PlayerPrefs.HasKey(HighScoreKey))
        {
            isNewRecord = true;
        }
        else
        {
            int oldScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            float oldTime = PlayerPrefs.GetFloat(HighScoreTimeKey, 0f);

            if (currentScore > oldScore)
            {
                isNewRecord = true;
            }
            else if (currentScore == oldScore && gameTimer < oldTime)
            {
                isNewRecord = true;
            }
        }

        if (isNewRecord)
        {
            PlayerPrefs.SetInt(HighScoreKey, currentScore);
            PlayerPrefs.SetFloat(HighScoreTimeKey, gameTimer);
            PlayerPrefs.Save();
            Debug.Log($"New High Score Saved: {currentScore} in {gameTimer}s");
        }
    }

    private void ResetGhostsToStart()
    {
        if (ghosts == null || ghostStartPositions == null) return;

        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] != null && i < ghostStartPositions.Length)
            {
                ghosts[i].transform.position = ghostStartPositions[i];
                ghosts[i].SetState(GhostState.Normal);
                ghosts[i].SetDirection(GhostDirection.Down);
            }
        }
    }

    private void AddScore(int amount)
    {
        currentScore += amount;

        if (uiManager != null)
        {
            uiManager.UpdateScoreUI(currentScore);
        }
    }

    private void CheckLevelComplete()
    {
        if (totalPelletsRemaining <= 0)
        {
            EndGame();
        }
    }
}