using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    private static bool hasSavedXpState;
    private static int savedLevel;
    private static int savedCurrentXP;
    private static int savedXPToLevel;

    [Header("Persistence")]
    [Tooltip("Enable only if XPManager lives on a dedicated manager object, not on Player.")]
    [SerializeField] private bool persistAcrossScenes = false;

    private void Awake()
    {
        if (!persistAcrossScenes)
        {
            Instance = this;
            RestoreXpState();
            return;
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple XPManager instances detected — destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RestoreXpState();
    }

    private void OnDestroy()
    {
        SaveXpState();
        if (Instance == this)
            Instance = null;
    }
    public int level;
    public int currentXP;
    public int XPToLevel = 10;
    public float XPGrowthMultiplier = 1.2f;
    public int healthIncreasePerLevel = 10;
    public Slider XPSlider;
    public TMP_Text currentLevelText;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SaveXpState();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        EnsureUIReferences();
        UpdateUI();
    }
    
    public void GainExperience(int amount)
    {
        currentXP += amount;
        SaveXpState();
        UpdateUI();
        while (currentXP >= XPToLevel)
        {
            LevelUp();
        }
    }

    public static void AddExperience(int amount)
    {
        if (Instance == null)
        {
            Debug.LogWarning("XPManager.Instance is null. Attempting to locate an XPManager in the scene.");
            XPManager found = FindObjectOfType<XPManager>();
            if (found != null)
            {
                Instance = found;
            }
            else
            {
                Debug.LogWarning("No XPManager found to add experience.");
                return;
            }
        }

        Instance.GainExperience(amount);
    }
    
    private void LevelUp()
    {
        level++;
        currentXP -= XPToLevel;
        XPToLevel = Mathf.RoundToInt(XPToLevel * XPGrowthMultiplier);
        SaveXpState();

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(healthIncreasePerLevel, true);
        }
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (XPSlider == null || currentLevelText == null)
        {
            EnsureUIReferences();
            if (XPSlider == null || currentLevelText == null)
            {
                Debug.LogWarning("XPManager: UI references not assigned; cannot update XP UI.");
                return;
            }
        }

        XPSlider.maxValue = XPToLevel;
        XPSlider.value = currentXP;
        currentLevelText.text = "Level: " + level;

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureUIReferences();
        UpdateUI();
    }

    private void SaveXpState()
    {
        hasSavedXpState = true;
        savedLevel = level;
        savedCurrentXP = currentXP;
        savedXPToLevel = XPToLevel;
    }

    private void RestoreXpState()
    {
        if (!hasSavedXpState)
            return;

        level = savedLevel;
        currentXP = savedCurrentXP;
        XPToLevel = Mathf.Max(1, savedXPToLevel);
    }

    private void EnsureUIReferences()
    {
        if (XPSlider == null)
        {
            GameObject sObj = GameObject.Find("XPSlider");
            if (sObj != null) 
                XPSlider = sObj.GetComponent<Slider>();
        }

        if (currentLevelText == null)
        {
            GameObject tObj = GameObject.Find("LevelText");
            if (tObj != null) 
                currentLevelText = tObj.GetComponent<TMP_Text>();
            
        }
    }
}
