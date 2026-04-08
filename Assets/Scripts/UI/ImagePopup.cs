using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ImagePopup : MonoBehaviour
{
    [SerializeField] private Image popupImage;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private bool pauseGameWhileVisible = true;

    [Header("Progression")]
    [Tooltip("Set to 0 to not affect progress. Set to 1+ to unlock next level on completion.")]
    public int progressLevelToSet = 0;

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

    private void SaveProgress()
    {
        if (progressLevelToSet > 0)
        {
            int current = PlayerPrefs.GetInt("GameProgress", 0);
            if (progressLevelToSet > current)
            {
                PlayerPrefs.SetInt("GameProgress", progressLevelToSet);
                Debug.Log($"Level complete! GameProgress set to {progressLevelToSet}");
            }
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string completedScenes = PlayerPrefs.GetString("CompletedScenes", "");
        string sceneToken = $",{currentScene},";

        if (!$",{completedScenes},".Contains(sceneToken))
        {
            completedScenes += (completedScenes.Length > 0 ? "," : "") + currentScene;
            PlayerPrefs.SetString("CompletedScenes", completedScenes);
            Debug.Log($"Scene '{currentScene}' marked as completed.");
        }

        PlayerPrefs.Save();
    }

    public void ShowImage()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        SaveProgress();

        StopCurrentDisplayCoroutine();
        displayCoroutine = StartCoroutine(DisplayImage());
    }

    public void ShowImage(float duration)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        SaveProgress();

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
        Time.timeScale    = 0f;
        pausedByPopup     = true;
    }

    private void EndPause()
    {
        if (!pausedByPopup)
            return;

        Time.timeScale = previousTimeScale;
        pausedByPopup  = false;
    }

    private void OnDisable()
    {
        EndPause();
    }
}
