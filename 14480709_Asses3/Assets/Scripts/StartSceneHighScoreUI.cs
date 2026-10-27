using UnityEngine;
using TMPro;


public class StartSceneHighScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text level1HighScoreText;
    [SerializeField] private TMP_Text level1BestTimeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        int highScore = PlayerPrefs.GetInt("Level1HighScore", 0);

        float bestTime = PlayerPrefs.GetFloat("Level1BestTime", 0f);

        level1HighScoreText.text = "High Score: " + highScore.ToString("D6");

        level1BestTimeText.text = "Best Time: " + HUDController.FormatTime(bestTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
