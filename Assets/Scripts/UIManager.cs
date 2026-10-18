using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD UI Elements")]
    public TextMeshProUGUI scoreText;

    [Header("Lives UI Elements")]
    public GameObject[] lifeIcons;

    [Header("Countdown UI Elements")]
    public GameObject countdownOverlay;
    public TextMeshProUGUI countdownText;

    [Header("Game Timer UI Element")]
    public TextMeshProUGUI timerText;

    [Header("Ghost Scared Timer Element")]
    public TextMeshProUGUI scaredTimerText;

    public void GoToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void UpdateScoreUI(int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString("D6");
        }
        else
        {
            Debug.LogWarning("UIManager: scoreText reference is missing in the Inspector!");
        }
    }

    public void UpdateLivesUI(int currentLives)
    {
        if (lifeIcons == null) return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].SetActive(i < currentLives);
            }
        }
    }

    public void ShowCountdownText(string text)
    {
        if (countdownOverlay != null) countdownOverlay.SetActive(true);
        if (countdownText != null) countdownText.text = text;
    }

    public void HideCountdownOverlay()
    {
        if (countdownOverlay != null) countdownOverlay.SetActive(false);
    }

    public void UpdateGameTimerUI(float totalSeconds)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        int centiseconds = Mathf.FloorToInt((totalSeconds * 100f) % 100f);

        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, centiseconds);
    }

    public void ShowScaredTimer(float secondsLeft)
    {
        if (scaredTimerText != null)
        {
            if (!scaredTimerText.gameObject.activeSelf)
            {
                scaredTimerText.gameObject.SetActive(true);
            }
            scaredTimerText.text = Mathf.CeilToInt(secondsLeft).ToString();
        }
    }

    public void HideScaredTimer()
    {
        if (scaredTimerText != null && scaredTimerText.gameObject.activeSelf)
        {
            scaredTimerText.gameObject.SetActive(false);
        }
    }
}