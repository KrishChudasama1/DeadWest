using UnityEngine;

public class SetPlayerPosition : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Pushes the player this many units away from the trigger so they don't spawn in walls.")]
    public Vector3 spawnOffset = new Vector3(0f, 1.5f, 0f);

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Snap the Sheriff to the door PLUS the safe offset
            player.transform.position = transform.position + spawnOffset;

            // Fix Alpha (Make fully visible)
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white; 
            }

            // Fixes the paralysis and running glitch
            var moveScript = player.GetComponent<PlayerMovement>();
            if (moveScript != null) moveScript.enabled = true;

            var col = player.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;

            Animator anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f); 
                anim.SetBool("IsMoving", false); 
            }

            // Force the Camera to snap to the Sheriff (Z must be -10)
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
            }
        }
    }
}