using UnityEngine;
using System.Collections;

public class BuildingEntry : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("How fast the Sheriff fades out")]
    public float fadeSpeed = 1.5f;
    
    [Tooltip("Should the Sheriff stop moving once they enter?")]
    public bool lockMovement = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure the object entering has the "Player" tag
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeOutSheriff(other.gameObject));
        }
    }

    IEnumerator FadeOutSheriff(GameObject player)
    {
        SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();
        
        // If you have a PlayerController script, we disable it so they don't walk through the map
        if (lockMovement)
        {
            var movementScript = player.GetComponent<MonoBehaviour>(); // Generic way to find your controller
            if (movementScript != null) movementScript.enabled = false;
        }

        // Fading logic
        Color alphaColor = sprite.color;
        while (alphaColor.a > 0)
        {
            alphaColor.a -= Time.deltaTime * fadeSpeed;
            sprite.color = alphaColor;
            yield return null;
        }

        Debug.Log("Sheriff is now inside the building.");
        
        // Note: For Task 4, you can add SceneManager.LoadScene here to switch maps.
    }
}