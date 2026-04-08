using UnityEngine;

public abstract class InteractableItem : MonoBehaviour
{
    public string itemName;

    public abstract void OnPickup(); 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPickup();
            Destroy(gameObject); 
        }
    }
}