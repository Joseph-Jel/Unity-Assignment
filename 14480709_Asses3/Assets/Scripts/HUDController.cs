using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class HUDController : MonoBehaviour
{

    [Header("Main HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameTimerText;
    [SerializeField] private TMP_Text ghostScaredTimerText;
    [SerializeField] private Image[] lifeIcons;

    [Header("Round / Game Over")]
    [SerializeField] private GameObject blockingImage;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text gameOverText;

    [Header("Buttons")]
    [SerializeField] private Button[] hudButtons;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString("D6");
    }

    public void SetGameTime(float time)
    {
        gameTimerText.text = FormatTime(time);
    }
    public void SetLives(int lives)
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            lifeIcons[i].gameObject.SetActive(i < lives);
        }
    }

    public void SetGhostTimer(float time)
    {
        if (time < 0f)
        {
            HideGhostTimer();
            return;
        }

        ghostScaredTimerText.gameObject.SetActive(true);

        int seconds = Mathf.CeilToInt(time);

        ghostScaredTimerText.text = "SCARED: " + seconds;
    }

    public void HideGhostTimer()
    {
        ghostScaredTimerText.gameObject.SetActive(false);
    }

    public void ShowCountdown(string value)
    {
        blockingImage.SetActive(true);

        countdownText.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);

        countdownText.text = value;
    }

    public void HideCountdown()
    {
        countdownText.gameObject.SetActive(false);
        blockingImage.SetActive(false);
    }

    public void ShowGameOver()
    {
        blockingImage.SetActive(true);

        countdownText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
    }

    public void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in hudButtons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }

    public static string FormatTime(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        int hundredths = Mathf.FloorToInt((time - totalSeconds) * 100f );

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ExitToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}
