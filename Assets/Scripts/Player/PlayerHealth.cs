using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    private static bool hasPersistentState = false;
    private static int persistedMaxHealth;
    private static int persistedCurrentHealth;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public float invincibilityTime = 1f;

    [Header("UI")]
    public Image healthBar;
    public TextMeshProUGUI healthText;


    private bool isInvincible = false;

    private void Awake()
    {
        if (hasPersistentState)
        {
            maxHealth = Mathf.Max(1, persistedMaxHealth);
            currentHealth = Mathf.Clamp(persistedCurrentHealth, 0, maxHealth);
            return;
        }

        maxHealth = Mathf.Max(1, maxHealth);
        if (currentHealth <= 0 || currentHealth > maxHealth)
            currentHealth = maxHealth;

        SavePersistentState();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SavePersistentState();
    }

    void Start()
    {
        EnsureUIReferences();
        UpdateHealthBar();
    }
    
    

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        Revolver revolver = GetComponentInChildren<Revolver>();
        revolver?.CancelReload();

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        SavePersistentState();
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(InvincibilityFrames());
    }

    void UpdateHealthBar()
    {
        EnsureUIReferences();

        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        SavePersistentState();
        UpdateHealthBar();
    }
    
    public void IncreaseMaxHealth(int amount, bool refillHealth)
    {
        maxHealth += amount;
        if (refillHealth)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SavePersistentState();
        UpdateHealthBar();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureUIReferences();
        UpdateHealthBar();
    }

    private void EnsureUIReferences()
    {
        if (healthBar == null)
        {
            GameObject barObj = GameObject.Find("HealthBarFill");
            if (barObj != null)
                healthBar = barObj.GetComponent<Image>();
        }

        if (healthText == null)
        {
            GameObject textObj = GameObject.Find("HealthText");
            if (textObj != null)
                healthText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    private void SavePersistentState()
    {
        persistedMaxHealth = Mathf.Max(1, maxHealth);
        persistedCurrentHealth = Mathf.Clamp(currentHealth, 0, persistedMaxHealth);
        hasPersistentState = true;
    }

    System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            sr.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(invincibilityTime / 10);
            sr.color = Color.white;
            yield return new WaitForSeconds(invincibilityTime / 10);
        }

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player died!");
        GameOverManager.ShowGameOver();
    }
}
