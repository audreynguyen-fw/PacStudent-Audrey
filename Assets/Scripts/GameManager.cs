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
    public Transform[] ghostTransforms;
    private Vector3[] ghostStartPositions;

    [Header("Audio Settings")]
    public AudioSource bgmAudioSource;
    public AudioClip introMusic;
    public AudioClip normalBackgroundMusic;

    [Header("Game State")]
    public GameState currentState { get; private set; } = GameState.Countdown;
    public int currentScore { get; private set; } = 0;
    public int totalPelletsRemaining { get; private set; } = 0;
    public int lives { get; private set; } = StartingLives;

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
        lives = StartingLives;
        if (pacStudent == null)
        {
            Debug.LogWarning("GameManager: pacStudent reference is missing in the Inspector!");
        }

        if (ghostTransforms == null || ghostTransforms.Length < 4)
        {
            Debug.LogWarning("GameManager: ghostTransforms is missing or does not contain all 4 Ghosts in the Inspector!");
        }
        else
        {
            ghostStartPositions = new Vector3[ghostTransforms.Length];
            for (int i = 0; i < ghostTransforms.Length; i++)
            {
                if (ghostTransforms[i] != null)
                {
                    ghostStartPositions[i] = ghostTransforms[i].position;
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

        StartCoroutine(PacStudentDeathSequence());
    }

    private IEnumerator PacStudentDeathSequence()
    {
        currentState = GameState.PacStudentDying;

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
        if (ghostTransforms == null || ghostStartPositions == null) return;

        for (int i = 0; i < ghostTransforms.Length; i++)
        {
            if (ghostTransforms[i] != null && i < ghostStartPositions.Length)
            {
                ghostTransforms[i].position = ghostStartPositions[i];
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