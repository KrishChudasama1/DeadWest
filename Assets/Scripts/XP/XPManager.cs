using UnityEngine;
using TMPro;
using NUnit.Framework.Internal;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    public int level;
    public int currentXP;
    public int XPToLevel = 10;
    public float XPGrowthMultiplier = 1.2f;
    public Slider XPSlider;
    public TMP_Text currentLevelText;

    private void Start()
    {
        UpdateUI();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GainExperience(2);
        }
        UpdateUI();
    }
    
    public void GainExperience(int amount)
    {
        currentXP += amount;
        if(currentXP >= XPToLevel)
        {
            LevelUp();
        }
    }
    private void LevelUp()
    {
        level++;
        currentXP -= XPToLevel;
        XPToLevel = Mathf.RoundToInt(XPToLevel * XPGrowthMultiplier);

        HealthManager healthManager = GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.maxHealth += 20;
            healthManager.currentHealth += 20;
            healthManager.UpdateUI();
        }
    }
    public void UpdateUI()
    {
        XPSlider.maxValue = XPToLevel;
        XPSlider.value = currentXP;
        currentLevelText.text = "Level: " + level;

    }
}