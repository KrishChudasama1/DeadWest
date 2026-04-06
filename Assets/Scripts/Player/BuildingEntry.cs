using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingEntry : MonoBehaviour
{
    public float fadeSpeed = 1.2f;
    public float walkDistance = 0.75f;
    public string sceneToLoad; 
    public bool isLocked = true; 
    
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

                // --- SAVE THE BOOKMARK ---
                string currentScene = SceneManager.GetActiveScene().name;
                if (currentScene == "MainScene" || currentScene == "Street") 
                {
                    PlayerPrefs.SetFloat("HubX", sheriff.transform.position.x);
                    PlayerPrefs.SetFloat("HubY", sheriff.transform.position.y);
                    PlayerPrefs.SetInt("ReturningToHub", 1); 
                }

                SceneManager.LoadScene(sceneToLoad); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isEntering)
        {
            if (isLocked)
            {
                Debug.Log("Door is locked!");
                return; 
            }

            sheriff = other.gameObject;
            sheriffRender = sheriff.GetComponent<SpriteRenderer>();
            targetPos = sheriff.transform.position + new Vector3(0, walkDistance, 0);

            // Halt the animation to prevent the "Running Glitch"
            Animator anim = sheriff.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f); 
                anim.SetBool("IsMoving", false); 
            }

            // Disable movement and make the collider a trigger so they safely fade out
            var moveScript = sheriff.GetComponent<PlayerMovement>(); 
            if (moveScript != null) moveScript.enabled = false;

            var col = sheriff.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            isEntering = true;
            progress = 0f;
        }
    }
}