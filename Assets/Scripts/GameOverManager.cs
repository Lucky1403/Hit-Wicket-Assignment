using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip uiButtonAudioClip;
    [SerializeField] private AudioSource audioSource;

    private bool gameOver = false;
    private bool isRestarting = false;

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
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        int finalScore = 0;

        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.CurrentScore;
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = finalScore.ToString();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        if (isRestarting)
            return;

        StartCoroutine(RestartGameRoutine());
    }

    private IEnumerator RestartGameRoutine()
    {
        isRestarting = true;

        if (audioSource != null && uiButtonAudioClip != null)
        {
            audioSource.PlayOneShot(uiButtonAudioClip);

            yield return new WaitForSecondsRealtime(uiButtonAudioClip.length);
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}