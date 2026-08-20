using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 2f;
    [Header("Sound Settings")]
    [SerializeField] private AudioClip UIButtonAudioClip;
    [SerializeField] private AudioSource audioSource;

    private bool isLoading;

    private static MainMenuController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }
    }

    public void PlayGame()
    {
        if (isLoading)
            return;

        if (fadePanel == null)
        {
            Debug.LogError("MainMenuController: fadePanel is not assigned.", this);
            return;
        }
        audioSource.PlayOneShot(UIButtonAudioClip);
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        isLoading = true;

        fadePanel.blocksRaycasts = true;

        yield return StartCoroutine(Fade(0f, 1f));

        yield return SceneManager.LoadSceneAsync(1);

        yield return null;

        yield return StartCoroutine(Fade(1f, 0f));

        fadePanel.blocksRaycasts = false;
        isLoading = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            fadePanel.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        fadePanel.alpha = endAlpha;
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting");
        audioSource.PlayOneShot(UIButtonAudioClip);
        Application.Quit();
    }
}