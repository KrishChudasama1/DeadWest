using UnityEngine;

public class SetPlayerPosition : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0f, 0f, 0f); 
    
    [Header("Visual Fixes")]
    [Tooltip("Type the exact name of the Sorting Layer for this room (e.g., 'saloon' or 'Default')")]
    public string newSortingLayer = "Default";

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            player.transform.position = transform.position + spawnOffset;

            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white; 
                sr.sortingLayerName = newSortingLayer;
            }

            var moveScript = player.GetComponent<PlayerMovement>();
            if (moveScript != null) moveScript.enabled = true;

            var col = player.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
            }
        }
    }
}