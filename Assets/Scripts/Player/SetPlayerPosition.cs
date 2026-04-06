using UnityEngine;

public class SetPlayerPosition : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0f, 0f, 0f); 
    
    [Header("Visual Fixes")]
    [Tooltip("Type the exact name of the Sorting Layer for this room (e.g., 'saloon' or 'Default')")]
    public string newSortingLayer = "Default"; // <--- WE ADDED THIS

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Move Sheriff
            player.transform.position = transform.position + spawnOffset;

            // Force Visibility and UPDATE SORTING LAYER
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white; 
                sr.sortingLayerName = newSortingLayer; // fix sorting layer issue
            }

            // Enable Movement
            var moveScript = player.GetComponent<PlayerMovement>();
            if (moveScript != null) moveScript.enabled = true;

            var col = player.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;

            // Snap Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
            }
        }
    }
}