using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;
    public TMP_Text healthText;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }
    
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    private void Die()
    {
        Debug.Log("Player died!");
    }

    public void UpdateUI()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;
    }
}