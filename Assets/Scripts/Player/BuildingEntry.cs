using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BuildingEntry : MonoBehaviour
{
    [Header("Door Settings")]
    public float fadeDuration = 0.8f;
    public string sceneToLoad;

    [Header("Prompt")]
    public KeyCode enterKey  = KeyCode.E;
    public float fontSize    = 24f;
    public Color promptColor = Color.white;

    [Header("Local Lock (From Street Manager)")]
    public bool isLocked = false;

    [Header("Progression System")]
    [Tooltip("0 = Unlocked from start. 1 = Requires beating level 1, etc.")]
    public int requiredProgressLevel = 0;

    private GameObject     sheriff;
    private SpriteRenderer sheriffRender;
    private bool isEntering = false;
    private bool playerNear = false;
    private string promptMessage = "[E] Enter";

    private PlayerMovement _playerMovement;

    private Canvas   _canvas;
    private TMP_Text _promptText;

    private void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        promptMessage = (currentScene != "MainScene" && currentScene != "Street")
            ? "[E] Exit"
            : "[E] Enter";

        BuildPromptUI();
    }

    private void BuildPromptUI()
    {
        GameObject canvasGO = new GameObject("BuildingEntryCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject textGO = new GameObject("PromptText");
        textGO.transform.SetParent(canvasGO.transform, false);

        _promptText           = textGO.AddComponent<TextMeshProUGUI>();
        _promptText.text      = promptMessage;
        _promptText.fontSize  = fontSize;
        _promptText.color     = promptColor;
        _promptText.alignment = TextAlignmentOptions.Center;

        RectTransform rt    = textGO.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 80f);
        rt.sizeDelta        = new Vector2(400f, 60f);

        _promptText.gameObject.SetActive(false);
    }

    
    private void Update()
    {
        if (_promptText != null)
            _promptText.gameObject.SetActive(playerNear && !isEntering);

        if (playerNear && !isEntering && Input.GetKeyDown(enterKey))
            TryEnter();
    }

    
    private void TryEnter()
    {
        if (sheriff == null) return;

        if (isLocked)
        {
            Debug.Log("Door is locked by the StreetLevelManager!");
            return;
        }

        int currentSaveProgress = PlayerPrefs.GetInt("GameProgress", 0);
        if (currentSaveProgress < requiredProgressLevel)
        {
            Debug.Log($"Door locked! Need progress level {requiredProgressLevel}.");
            return;
        }

        sheriffRender = sheriff.GetComponent<SpriteRenderer>();
        if (sheriffRender == null) return;

        isEntering = true;

        // Lock movement immediately
        _playerMovement = sheriff.GetComponent<PlayerMovement>();
        if (_playerMovement != null) _playerMovement.SetMovementLocked(true);

        if (_promptText != null)
            _promptText.gameObject.SetActive(false);

        StartCoroutine(FadeAndLoad());
    }

    
    private IEnumerator FadeAndLoad()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (sheriffRender != null)
            {
                Color c = sheriffRender.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sheriffRender.color = c;
            }

            yield return null;
        }

       
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainScene" || currentScene == "Street")
        {
            PlayerPrefs.SetFloat("HubX", sheriff != null ? sheriff.transform.position.x : 0f);
            PlayerPrefs.SetFloat("HubY", sheriff != null ? sheriff.transform.position.y : 0f);
            PlayerPrefs.SetInt("ReturningToHub", 1);
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isEntering)
        {
            sheriff    = other.gameObject;
            playerNear = true;

            if (_promptText != null)
                _promptText.text = promptMessage;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            
            if (isEntering && _playerMovement != null)
                _playerMovement.SetMovementLocked(false);

            isEntering = false;
            sheriff    = null;

            if (sheriffRender != null)
            {
                Color c = sheriffRender.color;
                c.a = 1f;
                sheriffRender.color = c;
            }

            if (_promptText != null)
                _promptText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_canvas != null)
            Destroy(_canvas.gameObject);
    }
}