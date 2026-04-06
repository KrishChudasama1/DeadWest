using UnityEngine;

public class HubReturn : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("ReturningToHub", 0) == 1)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Snap player back to the bookmarked door coordinates
                float returnX = PlayerPrefs.GetFloat("HubX");
                float returnY = PlayerPrefs.GetFloat("HubY");
                player.transform.position = new Vector3(returnX, returnY - 1.5f, 0f);
                
                // Fix Alpha (Make visible)
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f; 
                    sr.color = c;
                }

                // Activate the sheriff
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
            }
            
            PlayerPrefs.SetInt("ReturningToHub", 0);
        }
    }
}