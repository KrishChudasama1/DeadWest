using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public float invincibilityTime = 1f;

    [Header("UI")]
    public Image healthBar;
    public TextMeshProUGUI healthText;


    private bool isInvincible = false;

    private static bool hasSavedHealthState;
    private static int savedMaxHealth;

    private void Awake()
    {
        if (hasSavedHealthState)
            maxHealth = savedMaxHealth;

        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
    }

    void Start()
    {
        UpdateHealthBar();
    }
    
    

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        Revolver revolver = GetComponentInChildren<Revolver>();
        revolver?.CancelReload();

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(InvincibilityFrames());
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthBar();
    }
    
    public void IncreaseMaxHealth(int amount, bool refillHealth)
    {
        maxHealth = Mathf.Max(1, maxHealth + amount);
        SaveHealthState();

        if (refillHealth)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    public void RespawnToFullHealth()
    {
        StopAllCoroutines();
        isInvincible = false;

        currentHealth = maxHealth;
        UpdateHealthBar();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.white;
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
        SaveHealthState();
        Debug.Log("Player died!");
        GameOverManager.ShowGameOver();
    }

    private void SaveHealthState()
    {
        hasSavedHealthState = true;
        savedMaxHealth = Mathf.Max(1, maxHealth);
    }
}
