using UnityEngine;

public class StreetLevelManager : MonoBehaviour
{
    public static StreetLevelManager Instance;
    
    [Header("Progression")]
    public int cluesRequired = 5;
    private int currentClues = 0;

    [Header("The Sequence")]
    
    public BuildingEntry[] orderedDoors; 

    void Awake() { Instance = this; }

    void Start()
    {
        
        UpdateDoors();
    }

    
    public void AddClue(string clueName)
    {
        
        if (PlayerPrefs.GetInt("HubProgress", 0) > 0) return;

        currentClues++;
        Debug.Log("Clue found! " + currentClues + "/" + cluesRequired);

        if (currentClues >= cluesRequired)
        {
            
            PlayerPrefs.SetInt("HubProgress", 1);
            UpdateDoors();
        }
    }

    void UpdateDoors()
    {
        int progressStage = PlayerPrefs.GetInt("HubProgress", 0);

        
        foreach (BuildingEntry door in orderedDoors)
        {
            door.isLocked = true;
        }

        
        if (progressStage > 0 && progressStage <= orderedDoors.Length)
        {
            
            orderedDoors[progressStage - 1].isLocked = false;
            Debug.Log("Door number " + progressStage + " is now UNLOCKED!");
        }
        else if (progressStage > orderedDoors.Length)
        {
            
            Debug.Log("All levels complete. The final showdown begins!");
            
        }
    }
    
    public int GetCluesFound() 
    { 
        return currentClues; 
    }
}