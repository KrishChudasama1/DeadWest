using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingEntry : MonoBehaviour
{
    public float fadeSpeed = 1.2f;
    public float walkDistance = 0.75f;
    public string sceneToLoad; // exact name of team member scene 
    public bool isLocked = true; // Controlled by the StreetLevelManager
    
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
            sheriff.transform.position = Vector3.Lerp(sheriff.transform.position, targetPos, progress);
            
            Color c = sheriffRender.color;
            c.a = Mathf.Lerp(1f, 0f, progress);
            sheriffRender.color = c;

            if (progress >= 1f)
            {
                isEntering = false;
                SceneManager.LoadScene(sceneToLoad); // Move to team member level
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isEntering)
        {
            if (isLocked)
            {
                Debug.Log("Door is locked! Finish the current objective.");
                return; // Stop here if locked
            }

            sheriff = other.gameObject;
            sheriffRender = sheriff.GetComponent<SpriteRenderer>();
            targetPos = sheriff.transform.position + new Vector3(0, walkDistance, 0);

            var moveScript = sheriff.GetComponent<MonoBehaviour>(); 
            if (moveScript != null) moveScript.enabled = false;

            var col = sheriff.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            isEntering = true;
            progress = 0f;
        }
    }
}