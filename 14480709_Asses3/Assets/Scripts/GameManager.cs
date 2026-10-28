using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private HUDController hud;
    [SerializeField] private PacStudentController pacStudent;
    [SerializeField] private GhostStateController[] ghostStates;
    [SerializeField] private AudioManager audioManager;

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float scaredDuration = 10f;
    [SerializeField] private float recoveringTime = 3f;
    [SerializeField] private float deathSequenceDuration = 2f;

    private int score;
    private int lives;
    private int remainingPellets;

    private float gameTime;
    private float ghostScaredTimer;

    private bool gameStarted;
    private bool roundActive;
    private bool gameOver;
    private bool recoveringApplied;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        score = 0;
        lives = startingLives;
        gameTime = 0f;

        gameStarted = false;
        roundActive = false;
        gameOver = false;
        recoveringApplied = false;

        if (hud != null)
        {
            hud.SetScore(score);
            hud.SetLives(lives);
            hud.SetGameTime(0f);
            hud.HideGhostTimer();
            hud.SetButtonsInteractable(true);
        }

        if (pacStudent != null)
        {
            pacStudent.SetControlEnabled(false);
            pacStudent.SetCollisionEnabled(false);
        }

        SetGhostMovement(false);

        yield return null;

        CountRemainingPellets();

        yield return StartCoroutine(RoundStartCountdown());
    }

    // Update is called once per frame
    private void Update()
    {
        if (gameStarted && !gameOver)
        {
            gameTime += Time.deltaTime;

            if (hud != null)
            {
                hud.SetGameTime(gameTime);
            }
        }

        if (roundActive && !gameOver && ghostScaredTimer > 0f)
        {
            UpdateGhostScaredTimer();
        }
    }

    public void CollectPellet()
    {
        if (!roundActive || gameOver)
        {
            return;
        }

        AddScore(10);
        RegisterPelletRemoved();
    }

    public void CollectCherry()
    {
        if (!roundActive || gameOver)
        {
            return;
        }

        AddScore(100);

        if (audioManager != null)
        {
            audioManager.PlayCherryEat();
        }
    }

    public void CollectPowerPellet()
    {
        if (!roundActive || gameOver)
        {
            return;
        }

        AddScore(50);
        RegisterPelletRemoved();

        ghostScaredTimer = scaredDuration;
        recoveringApplied = false;

        foreach (GhostStateController ghost in ghostStates)
        {
            if (ghost != null && ghost.CurrentState != GhostState.Dead)
            {
                ghost.SetState(GhostState.Scared);
            }
        }

        hud.SetGhostTimer(ghostScaredTimer);

        RefreshBackgroundMusic();
    }

    private void AddScore(int amount)
    {
        score += amount;

        if (hud != null)
        {
            hud.SetScore(score);
        }
    }

    private void CountRemainingPellets()
    {
        int pelletCount = 0;
        int powerPelletCount = 0;

        GameObject[] pellets =
            GameObject.FindGameObjectsWithTag("Pellet");

        GameObject[] powerPellets =
            GameObject.FindGameObjectsWithTag("PowerPellet");

        pelletCount = pellets.Length;
        powerPelletCount = powerPellets.Length;

        remainingPellets = pelletCount + powerPelletCount;

        Debug.Log("Remaining pellets at start: " + remainingPellets);
    }

    private void RegisterPelletRemoved()
    {
        remainingPellets--;

        if (remainingPellets <= 0 && !gameOver)
        {
            StartCoroutine(GameOverRoutine());
        }
    }

    private IEnumerator RoundStartCountdown()
    {
        roundActive = false;
        gameStarted = false;

        if (pacStudent != null)
        {
            pacStudent.SetControlEnabled(false);
            pacStudent.SetCollisionEnabled(false);
        }

        SetGhostMovement(false);

        if (audioManager != null)
        {
            audioManager.PlayIntroMusic();
        }

        string[] countdownValues = { "3", "2", "1", "GO!" };

        foreach (string value in countdownValues)
        {
            if (hud != null)
            {
                hud.ShowCountdown(value);
            }

            yield return new WaitForSeconds(1f);
        }

        if (hud != null)
        {
            hud.HideCountdown();
        }

        if (audioManager != null)
        {
            audioManager.StopMusic();
            audioManager.PlayIntroMusic();
        }

        if (pacStudent != null)
        {
            pacStudent.SetCollisionEnabled(true);
            pacStudent.SetControlEnabled(true); 
        }

        SetGhostMovement(true); 

        gameStarted = true;
        roundActive = true; 
    }

    private void UpdateGhostScaredTimer()
    {
        ghostScaredTimer -= Time.deltaTime;

        if (ghostScaredTimer < 0f)
        {
            ghostScaredTimer = 0f;
        }

        if (hud != null)
        {
            hud.SetGhostTimer(ghostScaredTimer);
        }

        if (ghostScaredTimer <= recoveringTime && ghostScaredTimer > 0f && !recoveringApplied)
        {
            recoveringApplied = true;

            foreach (GhostStateController ghost in ghostStates)
            {
                if (ghost == null)
                {
                    continue;
                }

                if (ghost.CurrentState != GhostState.Dead)
                {
                    ghost.SetState(GhostState.Recovering);
                }
            }
        }

        if (ghostScaredTimer <= 0f)
        {
            ghostScaredTimer = 0f; 

            foreach (GhostStateController ghost in ghostStates)
            {
                if (ghost == null)
                {
                    continue;
                }

                if (ghost.CurrentState != GhostState.Dead)
                {
                    ghost.SetState(GhostState.Normal);
                }
            }

            if (hud != null)
            {
                hud.HideGhostTimer();
            }

            RefreshBackgroundMusic();
        }
    }

    public void HandleGhostCollision(GhostStateController ghost)
    {
        if (!roundActive || gameOver || ghost == null)
        {
            return;
        }

        switch (ghost.CurrentState)
        {
            case GhostState.Normal:
                StartCoroutine(PacStudentDeathRoutine());
                break;

            case GhostState.Scared:
            case GhostState.Recovering:
                EatGhost(ghost);
                break;

            case GhostState.Dead:
                break;
        }
    } 
    
    private void EatGhost(GhostStateController ghost)
    {

        ghost.SetState(GhostState.Dead);

        AddScore(300);

        audioManager.PlayGhostEat();


        RefreshBackgroundMusic();
    }

    private IEnumerator PacStudentDeathRoutine()
    {
        if (!roundActive || gameOver)
        {
            yield break;
        }

        roundActive = false;

        if (pacStudent != null)
        {
            pacStudent.SetControlEnabled(false);
            pacStudent.SetCollisionEnabled(false);
        }

        SetGhostMovement(false);

        ghostScaredTimer = 0f;
        recoveringApplied = false;

        if (hud != null)
        {
            hud.HideGhostTimer();
        }

        if (audioManager != null)
        {
            audioManager.StopMusic();
            audioManager.PlayPacStudentDeath();
        }

        if (pacStudent != null)
        {
            pacStudent.PlayDeath();
        }

        lives--;

        if (hud != null)
        {
            hud.SetLives(lives);
        }

        yield return new WaitForSeconds(deathSequenceDuration); 

        if (lives <= 0)
        {
            yield return StartCoroutine(GameOverRoutine());

            yield break;
        }

        if (pacStudent != null)
        {
            pacStudent.ResetToStart();
        }

        foreach (GhostStateController ghost in ghostStates)
        {
            if (ghost != null)
            {
                ghost.ResetGhost();
            }
        }

        if (audioManager != null)
        {
            audioManager.PlayNormalMusic();
        }

        if (pacStudent != null)
        {
            pacStudent.SetCollisionEnabled(true);
            pacStudent.SetControlEnabled(true);
        }

        SetGhostMovement(true);

        roundActive = true;
    }

    private void SetGhostMovement(bool enabled)
    {
        if (ghostStates == null)
        {
            return;
        }

        foreach (GhostStateController ghost in ghostStates)
        {
            if (ghost == null)
            {
                continue;
            }

            if (ghost.CurrentState != GhostState.Dead)
            {
                ghost.SetMovementEnabled(enabled);
            }
        }
    }

    public void RefreshBackgroundMusic()
    {
        if (audioManager == null)
        {
            return;
        }    

        bool deadGhostExists = false;

        foreach (GhostStateController ghost in ghostStates)
        {
            if (ghost == null)
            {
                continue;
            }

            if (ghost.CurrentState == GhostState.Dead)
            {
                deadGhostExists = true;
                break;
            }
        }

        if (deadGhostExists)
        {
            audioManager.PlayDeadMusic();
        }
        else if (ghostScaredTimer > 0f)
        {
            audioManager.PlayScaredMusic();
        }
        else
        {
            audioManager.PlayNormalMusic();
        }
    }

    private IEnumerator GameOverRoutine()
    {
        if (gameOver)
        {
            yield break;
        }

        gameOver = true;
        roundActive = false;
        gameStarted = false;

        if (pacStudent != null)
        {
            pacStudent.SetControlEnabled(false);
            pacStudent.SetCollisionEnabled(false);
        }

        SetGhostMovement(false);

        if (audioManager != null)
        {
            audioManager.StopMusic();
        }

        if (hud != null)
        {
            hud.SetButtonsInteractable(false);
            hud.ShowGameOver();
        }

        SaveHighScore();

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene("StartScene");
    }

    private void SaveHighScore()
    {
        const string scoreKey = "Level1HighScore";
        const string timeKey = "Level1BestTime";

        int previouScore = PlayerPrefs.GetInt(scoreKey, 0);

        bool hasPreviousTime = PlayerPrefs.HasKey(timeKey);

        float previousTime = PlayerPrefs.GetFloat(timeKey, 0f);

        bool higherScore = score > previouScore;

        bool sameScoreButFaster = score == previouScore && (!hasPreviousTime || gameTime < previousTime);

        if (higherScore || sameScoreButFaster)
        {
            PlayerPrefs.SetInt(scoreKey, score);

            PlayerPrefs.SetFloat(timeKey, gameTime);

            PlayerPrefs.Save();

            Debug.Log("New high score saved: " +  score + "Time: " + gameTime); 
        }
    }

    public GhostState GetGlobalGhostState()
    {
        if (ghostScaredTimer <= 0f)
        {
            return GhostState.Normal;
        }

        if (ghostScaredTimer <= recoveringTime)
        {
            return GhostState.Recovering;
        }

        return GhostState.Scared;
    }

    public void ReviveGhost(GhostStateController ghost)
    {
        GhostState state = GetGlobalGhostState();

        ghost.SetState(state);

        RefreshBackgroundMusic();
    }
}
