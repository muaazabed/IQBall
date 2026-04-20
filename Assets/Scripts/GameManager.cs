using UnityEngine;
using TMPro;

/// <summary>
/// Attach to an empty "GameManager" GameObject in the scene.
/// Singleton — accessed via GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    public int scoreToWin = 5;

    [Header("UI References (drag from Canvas)")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;
    public GameObject gameOverPanel;       // Panel that shows at game end
    public TextMeshProUGUI gameOverText;   // "You Win!" / "You Lose!"

    [Header("Ball Reference")]
    public SoccerBall ball;

    [Header("Characters (drag all 4 characters here)")]
    [Tooltip("Every player and enemy character on the field")]
    public GameObject[] allCharacters;

    [Header("Reset Delay")]
    [Tooltip("Seconds to wait after a goal before play resumes")]
    public float resetDelay = 1f;

    private int playerScore;
    private int enemyScore;
    private bool gameOver;

    // Snapshot of every character's starting position & rotation
    private Vector3[] startPositions;
    private Quaternion[] startRotations;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerScore = 0;
        enemyScore = 0;
        gameOver = false;
        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Snapshot every character's starting pose
        if (allCharacters != null)
        {
            startPositions = new Vector3[allCharacters.Length];
            startRotations = new Quaternion[allCharacters.Length];
            for (int i = 0; i < allCharacters.Length; i++)
            {
                startPositions[i] = allCharacters[i].transform.position;
                startRotations[i] = allCharacters[i].transform.rotation;
            }
        }
    }

    // ── Scoring ────────────────────────────────────

    public void PlayerScored()
    {
        if (gameOver) return;
        playerScore++;
        UpdateUI();
        CheckWin();
        if (!gameOver) Invoke(nameof(ResetRound), resetDelay);
    }

    public void EnemyScored()
    {
        if (gameOver) return;
        enemyScore++;
        UpdateUI();
        CheckWin();
        if (!gameOver) Invoke(nameof(ResetRound), resetDelay);
    }

    // ── Helpers ────────────────────────────────────

    private void UpdateUI()
    {
        if (playerScoreText != null) playerScoreText.text = playerScore.ToString();
        if (enemyScoreText != null) enemyScoreText.text = enemyScore.ToString();
    }

    private void CheckWin()
    {
        if (playerScore >= scoreToWin)
        {
            EndGame("You Win!");
        }
        else if (enemyScore >= scoreToWin)
        {
            EndGame("You Lose!");
        }
    }

    private void EndGame(string message)
    {
        gameOver = true;
        Time.timeScale = 0f; // Freeze gameplay

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = message;
    }

    private void ResetRound()
    {
        // Reset the ball
        if (ball != null)
            ball.ResetBall();

        // Reset every character to their starting position
        if (allCharacters != null)
        {
            for (int i = 0; i < allCharacters.Length; i++)
            {
                Rigidbody rb = allCharacters[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                allCharacters[i].transform.position = startPositions[i];
                allCharacters[i].transform.rotation = startRotations[i];
            }
        }
    }

    // ── Restart (call from a UI button) ───────────

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}