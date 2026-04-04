using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("References")]
    [Tooltip("Drag the CanvasGroup that sits on the fade Canvas.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Defaults")]
    [SerializeField] private float defaultFadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start fully transparent
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeCanvasGroup != null && fadeCanvasGroup.alpha >= 1f)
            StartCoroutine(FadeIn(defaultFadeDuration));
    }

   
    public IEnumerator FadeOut(float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    
    public IEnumerator FadeIn(float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
  
    
    public IEnumerator FadeOut() { return FadeOut(defaultFadeDuration); }
    public IEnumerator FadeIn()  { return FadeIn(defaultFadeDuration);  }
}