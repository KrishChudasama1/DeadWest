using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingEntry : MonoBehaviour
{
    [Header("Door Settings")]
    public float fadeSpeed = 1.2f;
    public float walkDistance = 0.75f;
    public string sceneToLoad; 

    // --- RESTORED TO FIX THE COMPILER ERROR ---
    [Header("Local Lock (From Street Manager)")]
    public bool isLocked = false; 

    [Header("Progression System")]
    [Tooltip("0 = Unlocked from start. 1 = Requires beating level 1, etc.")]
    public int requiredProgressLevel = 0; 
    
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

                // Save Hub Bookmark
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
            // 1. Check the old StreetLevelManager lock
            if (isLocked)
            {
                Debug.Log("Door is locked by the StreetLevelManager!");
                return; 
            }

            // 2. Check the new Save File progression lock
            int currentSaveProgress = PlayerPrefs.GetInt("GameProgress", 0);
            if (currentSaveProgress < requiredProgressLevel)
            {
                Debug.Log($"Door locked! You need Progress Level {requiredProgressLevel}, but you are only at Level {currentSaveProgress}.");
                return; 
            }

            sheriff = other.gameObject;
            sheriffRender = sheriff.GetComponent<SpriteRenderer>();
            targetPos = sheriff.transform.position + new Vector3(0, walkDistance, 0);

            Animator anim = sheriff.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f); 
                anim.SetBool("IsMoving", false); 
            }

            var moveScript = sheriff.GetComponent<PlayerMovement>(); 
            if (moveScript != null) moveScript.enabled = false;

            var col = sheriff.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            isEntering = true;
            progress = 0f;
        }
    }
}