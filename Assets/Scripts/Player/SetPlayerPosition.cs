using UnityEngine;

public class SetPlayerPosition : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // 1. Snap the Sheriff to the door
            player.transform.position = transform.position;

            // 2. Fix Alpha (Make visible)
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f; 
                sr.color = c;
            }

            // 3. WAKE UP THE SHERIFF (Fixes the paralysis and running glitch)
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

            // 4. Force the Camera to snap to the Sheriff (Z must be -10)
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
            }
        }
    }
}