using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    public int level;
    public int currentXP;
    public int XPToLevel = 10;
    public float XPGrowthMultiplier = 1.2f;
    public int healthIncreasePerLevel = 10;
    public Slider XPSlider;
    public TMP_Text currentLevelText;

    private void Start()
    {
        UpdateUI();
    }
    
    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GainExperience(2);
        }
        */
        UpdateUI();
    }
    
    public void GainExperience(int amount)
    {
        currentXP += amount;
        while (currentXP >= XPToLevel)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        level++;
        currentXP -= XPToLevel;
        XPToLevel = Mathf.RoundToInt(XPToLevel * XPGrowthMultiplier);

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>(); // changed this
        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(healthIncreasePerLevel, true);
        }
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        XPSlider.maxValue = XPToLevel;
        XPSlider.value = currentXP;
        currentLevelText.text = "Level: " + level;

    }
}