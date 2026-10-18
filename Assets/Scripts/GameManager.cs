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

    [Header("UI Reference")]
    public UIManager uiManager;

    [Header("PacStudent & Death References")]
    public PacStudentController pacStudent;
    public ParticleSystem deathParticle;
    public AudioSource sfxAudioSource;
    public AudioClip deathSound;
    public float deathAnimationDuration = 1.5f;

    [Header("Ghosts References")]
    public GhostController[] ghosts;
    private Vector3[] ghostStartPositions;

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
    private Coroutine scaredRoutine;

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

        if (ghosts == null || ghosts.Length < 4)
        {
            Debug.LogWarning("GameManager: ghosts array is missing or does not contain all 4 Ghosts in the Inspector!");
        }
        else
        {
            ghostStartPositions = new Vector3[ghosts.Length];
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

        PlayBgm(normalBackgroundMusic);
    }

    public void PlayBgm(AudioClip clip)
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
    }

    public void OnGhostEaten()
    {
        if (currentState != GameState.InGame) return;

        AddScore(ScoreGhost);
    }

    public void OnGhostTouched(GameObject ghost)
    {
        if (currentState != GameState.InGame) return;

        StartCoroutine(PacStudentDeathSequence());
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

        PlayBgm(scaredMusic);

        float remaining = scaredDuration;
        bool recovering = false;

        while (remaining > 0)
        {
            if (uiManager != null)
            {
                uiManager.ShowScaredTimer(remaining);
            }

            if (remaining <= recoverTime && !recovering)
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

            remaining -= Time.deltaTime;
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

        PlayBgm(normalBackgroundMusic);
        scaredRoutine = null;
    }

    private IEnumerator PacStudentDeathSequence()
    {
        currentState = GameState.PacStudentDying;

        if (scaredRoutine != null)
        {
            StopCoroutine(scaredRoutine);
            scaredRoutine = null;
        }

        if (uiManager != null)
        {
            uiManager.HideScaredTimer();
        }

        PlayBgm(normalBackgroundMusic);

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
            currentState = GameState.GameOver;
            Debug.Log("Game Over! All lives lost.");
        }
        else
        {
            if (pacStudent != null) pacStudent.ResetToStart();

            ResetGhostsToStart();

            currentState = GameState.InGame;
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
            currentState = GameState.GameOver;
            Debug.Log("Level Complete! All pellets consumed.");
        }
    }
}