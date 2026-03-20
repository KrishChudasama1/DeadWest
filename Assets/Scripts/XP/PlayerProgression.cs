using UnityEngine;
using System;

public class PlayerProgression : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 100;

    [Header("Scaling")]
    [SerializeField] private int xpGrowthPerLevel = 50;
    [SerializeField] private int healthIncreasePerLevel = 20;

    private PlayerHealth playerHealth;

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPToNextLevel => xpToNextLevel;

    public event Action<int, int, int> OnXPChanged;
    public event Action<int> OnLevelUp;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        NotifyXPChanged();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        NotifyXPChanged();
    }

    private void LevelUp()
    {
        currentLevel++;
        xpToNextLevel += xpGrowthPerLevel;

        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(healthIncreasePerLevel);
        }

        OnLevelUp?.Invoke(currentLevel);

        Debug.Log("Level Up! New Level: " + currentLevel);
    }

    private void NotifyXPChanged()
    {
        OnXPChanged?.Invoke(currentXP, xpToNextLevel, currentLevel);
    }
}