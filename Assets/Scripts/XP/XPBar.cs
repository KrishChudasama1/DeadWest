using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBar : MonoBehaviour
{
    public RectTransform fillRect;
    public TextMeshProUGUI levelText;

    public void UpdateBar(int currentXP, int maxXP, int level)
    {
        float percent = (float)currentXP / maxXP;
        fillRect.anchorMax = new Vector2(percent, 1f);

        if (levelText != null)
            levelText.text = "Level " + level;
    }
}