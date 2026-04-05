using UnityEngine;

public class SheriffClue : InteractableItem
{
    public override void OnPickup()
    {
        // counts clue
        if (StreetLevelManager.Instance != null)
        {
            StreetLevelManager.Instance.AddClue(itemName);
        }
        Debug.Log("Sheriff recovered: " + itemName);
    }
}