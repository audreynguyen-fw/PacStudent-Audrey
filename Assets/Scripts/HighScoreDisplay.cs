using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    [Header("UI Text Reference")]
    public TextMeshProUGUI level1ScoreText;

    private void Start()
    {
        DisplayHighScore();
    }

    public void DisplayHighScore()
    {
        if (level1ScoreText == null) return;

        if (PlayerPrefs.HasKey(GameManager.HighScoreKey))
        {
            int score = PlayerPrefs.GetInt(GameManager.HighScoreKey, 0);
            float totalSeconds = PlayerPrefs.GetFloat(GameManager.HighScoreTimeKey, 0f);

            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            int centiseconds = Mathf.FloorToInt((totalSeconds * 100f) % 100f);

            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, centiseconds);

            level1ScoreText.text = $"Level 1 - High Score: {score} Time: {formattedTime}";
        }
        else
        {
            level1ScoreText.text = "Level 1 - High Score: 0 Time: 00:00:00";
        }
    }
}