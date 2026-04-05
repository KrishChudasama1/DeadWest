using UnityEngine;

public class StreetLevelManager : MonoBehaviour
{
    public static StreetLevelManager Instance;

    public int cluesFound = 0;
    public int totalCluesRequired = 5;

    [Header("Story Triggers")]
    public GameObject hiddenBuilding; // Drag  hidden building here
    public GameObject formerFriendBoss; // Drag boss ghost here

    void Awake()
    {
        // This makes the script accessible from anywhere
        if (Instance == null) Instance = this;
    }

    public void AddClue(string name)
    {
        cluesFound++;
        Debug.Log("Clues: " + cluesFound + "/" + totalCluesRequired);

        if (cluesFound >= totalCluesRequired)
        {
            TriggerHiddenRevelation();
        }
    }

    void TriggerHiddenRevelation()
    {
        if (hiddenBuilding != null) hiddenBuilding.SetActive(true);
        if (formerFriendBoss != null) formerFriendBoss.SetActive(true);
        
        Debug.Log("The truth has surfaced. The Boss has appeared!");
    }
}