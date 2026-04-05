using UnityEngine;
using System.Collections;

public class GraveInteraction : MonoBehaviour
{
    public bool hasRelic = false;
    public GameObject enemyPrefab;
    public float digDuration = 2f;
    public GameObject interactPrompt;
    public Sprite dugSprite;

    private bool playerNearby = false;
    private bool hasBeenDug = false;
    private PlayerMovement playerMovement;
    private Animator playerAnimator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerMovement = other.GetComponentInParent<PlayerMovement>();
            playerAnimator = other.GetComponentInParent<Animator>();
            if (!hasBeenDug && interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerMovement = null;
            playerAnimator = null;
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerNearby && !hasBeenDug && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Dig());
        }
    }

    IEnumerator Dig()
    {
        hasBeenDug = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

        if (playerAnimator != null)
            playerAnimator.SetBool("IsDigging", true);

        yield return new WaitForSeconds(digDuration);

        if (playerAnimator != null)
            playerAnimator.SetBool("IsDigging", false);

        if (playerMovement != null)
            playerMovement.SetMovementLocked(false);

        if (dugSprite != null)
            spriteRenderer.sprite = dugSprite;

        if (hasRelic)
            Debug.Log("You found the relic!");
        else
        {
            if (enemyPrefab != null)
                Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }
    }
}