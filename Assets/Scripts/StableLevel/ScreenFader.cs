using System.Collections;
using UnityEngine;

/// <summary>
/// Provides full-screen fade-to-black transitions using a CanvasGroup alpha tween.
/// Attach to a Canvas that contains a full-screen Image with a CanvasGroup component.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    [Tooltip("The CanvasGroup on the full-screen black overlay panel.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Tooltip("Default duration for fade transitions in seconds.")]
    [SerializeField] private float defaultFadeDuration = 0.5f;

    private static ScreenFader _instance;

    /// <summary>
    /// Singleton instance for easy access from any script.
    /// </summary>
    public static ScreenFader Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Fades the screen to black over the specified duration.
    /// </summary>
    /// <param name="duration">Fade duration in seconds. Uses default if &lt;= 0.</param>
    public Coroutine FadeOut(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultFadeDuration;
        return StartCoroutine(Fade(0f, 1f, duration));
    }

    /// <summary>
    /// Fades the screen from black back to clear over the specified duration.
    /// </summary>
    /// <param name="duration">Fade duration in seconds. Uses default if &lt;= 0.</param>
    public Coroutine FadeIn(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultFadeDuration;
        return StartCoroutine(Fade(1f, 0f, duration));
    }

    /// <summary>
    /// Performs a full fade-out then fade-in transition with an action in between.
    /// </summary>
    /// <param name="onScreenBlack">Action to invoke while the screen is fully black.</param>
    /// <param name="fadeDuration">Duration for each half of the transition.</param>
    public Coroutine FadeOutIn(System.Action onScreenBlack, float fadeDuration = -1f)
    {
        if (fadeDuration <= 0f) fadeDuration = defaultFadeDuration;
        return StartCoroutine(FadeOutInRoutine(onScreenBlack, fadeDuration));
    }

    private IEnumerator FadeOutInRoutine(System.Action onScreenBlack, float fadeDuration)
    {
        yield return Fade(0f, 1f, fadeDuration);
        onScreenBlack?.Invoke();
        yield return Fade(1f, 0f, fadeDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
        fadeCanvasGroup.blocksRaycasts = to > 0.5f;
    }
}
