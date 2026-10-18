using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD UI Elements")]
    public TextMeshProUGUI scoreText;

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
}