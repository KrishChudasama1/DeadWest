using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPUI : MonoBehaviour
{
    [SerializeField] private PlayerProgression playerProgression;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;

    private void Start()
    {
        if (playerProgression != null)
        {
            playerProgression.OnXPChanged += UpdateUI;
            UpdateUI(
                playerProgression.CurrentXP,
                playerProgression.XPToNextLevel,
                playerProgression.CurrentLevel
            );
        }
    }

    private void OnDestroy()
    {
        if (playerProgression != null)
        {
            playerProgression.OnXPChanged -= UpdateUI;
        }
    }

    private void UpdateUI(int currentXP, int xpToNextLevel, int currentLevel)
    {
        xpSlider.maxValue = xpToNextLevel;
        xpSlider.value = currentXP;

        levelText.text = "Level " + currentLevel;
        xpText.text = currentXP + " / " + xpToNextLevel + " XP";
    }
}