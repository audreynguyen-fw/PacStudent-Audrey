using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD UI Elements")]
    public TextMeshProUGUI scoreText;

    [Header("Countdown UI Elements")]
    public GameObject countdownOverlay;
    public TextMeshProUGUI countdownText;

    [Header("Game Timer UI Element")]
    public TextMeshProUGUI timerText;

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
}