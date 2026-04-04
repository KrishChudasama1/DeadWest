using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("Exact name of the scene to load (must be in Build Settings).")]
    [SerializeField] private string sceneName;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading) return;
        if (!other.CompareTag("Player")) return;

        isLoading = true;

        
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // Fade to black
        if (ScreenFader.Instance != null)
            yield return StartCoroutine(ScreenFader.Instance.FadeOut(fadeDuration));

     
        SceneManager.LoadSceneAsync(sceneName);
    }
}