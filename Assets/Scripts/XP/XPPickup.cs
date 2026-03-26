using UnityEngine;

public class XPPickup : MonoBehaviour
{
    public int xpValue = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggered by: " + other.name + " | Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            XPManager xpManager = other.GetComponent<XPManager>();
            if (xpManager != null)
            {
                xpManager.GainExperience(xpValue);
                Destroy(gameObject);
            }
        }
    }

}