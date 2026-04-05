using UnityEngine;

public abstract class InteractableItem : MonoBehaviour
{
    public string itemName;

    //blueprint for what happens when the Sheriff picks it up
    public abstract void OnPickup(); 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPickup();
            // Clue disappears after collection
            Destroy(gameObject); 
        }
    }
}