using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImagePopup : MonoBehaviour
{
    [SerializeField] private Image popupImage;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private bool pauseGameWhileVisible = true;

    private Coroutine displayCoroutine;
    private bool pausedByPopup;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (popupImage == null)
            popupImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (popupImage != null)
            popupImage.enabled = false;
    }

    public void ShowImage()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        StopCurrentDisplayCoroutine();
        displayCoroutine = StartCoroutine(DisplayImage());
    }
  
    public void ShowImage(float duration)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        StopCurrentDisplayCoroutine();
        displayCoroutine = StartCoroutine(DisplayImageForDuration(duration));
    }

    private IEnumerator DisplayImage()
    {
        if (popupImage == null)
            yield break;

        popupImage.enabled = true;

        BeginPause();

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, displayDuration));

        popupImage.enabled = false;

        EndPause();
    }

    private IEnumerator DisplayImageForDuration(float duration)
    {
        if (popupImage == null)
            yield break;

        popupImage.enabled = true;

        BeginPause();

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration));

        popupImage.enabled = false;

        EndPause();
    }

    private void StopCurrentDisplayCoroutine()
    {
        if (displayCoroutine == null)
            return;

        StopCoroutine(displayCoroutine);
        displayCoroutine = null;

        if (popupImage != null)
            popupImage.enabled = false;

        EndPause();
    }

    private void BeginPause()
    {
        if (!pauseGameWhileVisible || pausedByPopup)
            return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pausedByPopup = true;
    }

    private void EndPause()
    {
        if (!pausedByPopup)
            return;

        Time.timeScale = previousTimeScale;
        pausedByPopup = false;
    }

    private void OnDisable()
    {
        EndPause();
    }
}