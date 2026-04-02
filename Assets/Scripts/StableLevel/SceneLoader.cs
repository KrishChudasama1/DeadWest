using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Triggers a scene transition when the player enters a trigger zone.
/// Attach to a GameObject with a Collider2D set to IsTrigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load when the player enters the trigger zone.")]
    [SerializeField] private string targetSceneName = "StableScene";

    [Tooltip("Duration of the fade-to-black transition before loading.")]
    [SerializeField] private float fadeDuration = 0.5f;

    private bool _isLoading;

    /// <summary>
    /// Loads the target scene with a fade transition when the player enters the trigger.
    /// </summary>
    /// <param name="other">The collider that entered the trigger zone.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isLoading) return;
        if (!other.CompareTag("Player")) return;

        _isLoading = true;
        StartCoroutine(LoadSceneWithFade());
    }

    private IEnumerator LoadSceneWithFade()
    {
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut(fadeDuration);
        else
            yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// Loads a scene by name with a fade transition. Can be called from any script.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public static void LoadScene(string sceneName)
    {
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.StartCoroutine(StaticLoadWithFade(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }

    private static IEnumerator StaticLoadWithFade(string sceneName)
    {
        yield return ScreenFader.Instance.FadeOut();
        SceneManager.LoadScene(sceneName);
    }
}
