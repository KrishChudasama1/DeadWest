using UnityEngine;

// Written by Mark Zhang - SE2250 Project Task 3
public class BuildingEntry : MonoBehaviour
{
    public float fadeSpeed = 1.2f;
    public float walkDistance = 0.75f;
    
    private GameObject sheriff;
    private SpriteRenderer sheriffRender;
    private bool isEntering = false;
    
    private Vector3 targetPos;
    private float progress = 0f; 

    void Update()
    {
        if (isEntering && sheriff != null)
        {
            progress += Time.deltaTime * fadeSpeed;

            // Move sheriff into building
            sheriff.transform.position = Vector3.Lerp(sheriff.transform.position, targetPos, progress);

            // Update transparency
            Color c = sheriffRender.color;
            c.a = Mathf.Lerp(1f, 0f, progress);
            sheriffRender.color = c;

            // End transition
            if (progress >= 1f)
            {
                isEntering = false;
                Debug.Log("Sheriff entered building");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Start transition if player hits the trigger
        if (other.CompareTag("Player") && !isEntering)
        {
            sheriff = other.gameObject;
            sheriffRender = sheriff.GetComponent<SpriteRenderer>();
            
            targetPos = sheriff.transform.position + new Vector3(0, walkDistance, 0);

            // Disable player control
            var moveScript = sheriff.GetComponent<MonoBehaviour>(); 
            if (moveScript != null) moveScript.enabled = false;

            // Allow ghosting through teammate's wall colliders
            var col = sheriff.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            isEntering = true;
            progress = 0f;
        }
    }
}