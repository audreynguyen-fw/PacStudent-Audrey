using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    [Header("UI Text References")]
    public TextMeshProUGUI level1ScoreText;
    public TextMeshProUGUI level2ScoreText;

    private void Start()
    {
        DisplayHighScore();
    }

    public void DisplayHighScore()
    {
        UpdateScoreText(level1ScoreText, "Level 1", GameManager.HighScoreKey, GameManager.HighScoreTimeKey);
        UpdateScoreText(level2ScoreText, "Level 2", GameManager.HighScoreKeyLevel2, GameManager.HighScoreTimeKeyLevel2);
    }

    private void UpdateScoreText(TextMeshProUGUI targetText, string levelLabel, string scoreKey, string timeKey)
    {
        if (targetText == null) return;

        if (PlayerPrefs.HasKey(scoreKey))
        {
            int score = PlayerPrefs.GetInt(scoreKey, 0);
            float totalSeconds = PlayerPrefs.GetFloat(timeKey, 0f);

            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            int centiseconds = Mathf.FloorToInt((totalSeconds * 100f) % 100f);

            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, centiseconds);

            targetText.text = $"{levelLabel} - High Score: {score} Time: {formattedTime}";
        }
        else
        {
            targetText.text = $"{levelLabel} - High Score: 0 Time: 00:00:00";
        }
    }
}