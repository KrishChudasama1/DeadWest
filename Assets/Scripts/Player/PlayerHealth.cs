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

    void Start()
    {
        currentHealth = maxHealth;
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
        maxHealth += amount;
        if (refillHealth)
            currentHealth = maxHealth;
        UpdateHealthBar();
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
    }
}
