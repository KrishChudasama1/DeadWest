using UnityEngine;
using TMPro; // This is Unity's text system!

public class ClueManager : MonoBehaviour
{
    [Header("Clue Settings")]
    public int totalCluesRequired = 3;
    private int cluesCollected = 0;

    [Header("UI Settings")]
    [Tooltip("Drag the UI Image that has the ImagePopup script here")]
    public ImagePopup relicPopupScript;
    
    [Tooltip("Drag your new Objective Text object here")]
    public TextMeshProUGUI objectiveText;

    [Header("Reward Settings")]
    public int newProgressLevel = 1;

    void Start()
    {
        // Set the text as soon as the level loads
        UpdateObjectiveText();
    }

    public void CollectClue()
    {
        cluesCollected++;
        Debug.Log("Clue collected! Total: " + cluesCollected + "/" + totalCluesRequired);

        // Update the screen text every time we grab a skull
        UpdateObjectiveText();

        if (cluesCollected >= totalCluesRequired)
        {
            UnlockRelic();
        }
    }

    private void UpdateObjectiveText()
    {
        // Safety check to make sure we actually linked the text object
        if (objectiveText != null)
        {
            if (cluesCollected < totalCluesRequired)
            {
                objectiveText.text = "Objective: Find 3 Skulls to unlock the Saloon (" + cluesCollected + "/" + totalCluesRequired + ")";
            }
            else
            {
                // What it says when they win!
                objectiveText.text = "Objective: Saloon Unlocked!";
            }
        }
    }

    private void UnlockRelic()
    {
        if (relicPopupScript != null)
        {
            relicPopupScript.ShowImage();
        }

        int currentProgress = PlayerPrefs.GetInt("GameProgress", 0);
        if (currentProgress < newProgressLevel)
        {
            PlayerPrefs.SetInt("GameProgress", newProgressLevel);
            PlayerPrefs.Save();
            Debug.Log("Game Progress updated to level: " + newProgressLevel);
        }
    }
}