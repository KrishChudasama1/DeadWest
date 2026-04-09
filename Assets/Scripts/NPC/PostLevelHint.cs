using System.Collections;
using UnityEngine;
using TMPro;


public class PostLevelHint : MonoBehaviour
{
    [Header("Ghost NPC")]
    [Tooltip("Prefab with a SpriteRenderer (the ghost guide). Will be spawned next to the player.")]
    public GameObject ghostPrefab;
    [Tooltip("Offset from the player where the ghost appears.")]
    public Vector2 spawnOffset = new Vector2(1.5f, 0f);

    [Header("UI")]
    [Tooltip("A TextMeshProUGUI element for the hint bubble (can be on a world-space or screen-space canvas).")]
    public TextMeshProUGUI hintText;
    [Tooltip("The parent panel/image of the hint text — hidden by default, shown when hint is active.")]
    public GameObject hintPanel;

    [Header("Timing")]
    public float ghostFadeInTime  = 0.5f;
    public float ghostFadeOutTime = 0.5f;
    public KeyCode dismissKey = KeyCode.E;

    [Header("Typewriter")]
    [Tooltip("Seconds between each character appearing.")]
    public float typeSpeed = 0.03f;
    [Tooltip("Optional typing sound (plays per character).")]
    public AudioClip typeSFX;
    [Range(0f,1f)] public float typeSFXVolume = 0.3f;

    [Header("Hints (indexed by GameProgress level)")]
    [Tooltip("Element 0 = progress 0, element 1 = progress 1, etc. Leave blank entries for levels with no hint.")]
    [TextArea(2, 4)]
    public string[] hints = new string[]
    {
        "",                                                                          // 0 — game start, no hint yet
        "The Saloon's doors are open now, Sheriff. Head inside and watch your back.", // 1 — skulls done → saloon
        "I hear horses whinnying at the Stable down the road. That's your next stop.",// 2 — saloon done → stable
        "The County Jail reeks of dark energy. Time to clean it out.",                // 3 — stable/race done → jail
        "Restless souls stir in the Graveyard. You best put 'em to rest.",           // 4 — jail done → graveyard
        "The Church holds the final relic. End this curse once and for all.",         // 5 — graveyard done → church
    };

    
    private GameObject  spawnedGhost;
    private bool        hintActive = false;

    
    private const string LAST_HINT_KEY = "LastHintShown";

    void Start()
    {
        // Hide UI by default
        if (hintPanel != null) hintPanel.SetActive(false);

        int progress    = PlayerPrefs.GetInt("GameProgress", 0);
        int lastShown   = PlayerPrefs.GetInt(LAST_HINT_KEY, -1);

        // Only show if we have a new, unshown hint for this progress level
        if (progress > 0 && progress != lastShown && progress < hints.Length && !string.IsNullOrEmpty(hints[progress]))
        {
            StartCoroutine(ShowHintSequence(progress));
        }
    }

    IEnumerator ShowHintSequence(int progressLevel)
    {
        hintActive = true;

        // Wait a short beat so the scene has settled
        yield return new WaitForSeconds(0.5f);

        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) yield break;

        // Spawn the ghost next to the player
        Vector3 ghostPos = playerObj.transform.position + (Vector3)spawnOffset;
        if (ghostPrefab != null)
        {
            spawnedGhost = Instantiate(ghostPrefab, ghostPos, Quaternion.identity);

            // Fade the ghost in
            SpriteRenderer sr = spawnedGhost.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;

                float elapsed = 0f;
                while (elapsed < ghostFadeInTime)
                {
                    elapsed += Time.deltaTime;
                    c.a = Mathf.Clamp01(elapsed / ghostFadeInTime);
                    sr.color = c;
                    yield return null;
                }
            }
        }

        // Show the hint panel and type out the text
        if (hintPanel != null) hintPanel.SetActive(true);
        if (hintText  != null)
        {
            hintText.text = "";
            yield return StartCoroutine(TypeText(hints[progressLevel]));
            // Append dismiss prompt after typing finishes
            hintText.text += "\n<size=70%><color=#aaaaaa>[E] Dismiss</color></size>";
        }

        // Wait for the player to dismiss
        yield return new WaitUntil(() => Input.GetKeyDown(dismissKey));

        // Record that we showed this hint
        PlayerPrefs.SetInt(LAST_HINT_KEY, progressLevel);
        PlayerPrefs.Save();

        // Hide the text
        if (hintPanel != null) hintPanel.SetActive(false);

        // Fade the ghost out and destroy
        if (spawnedGhost != null)
        {
            SpriteRenderer sr = spawnedGhost.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                float elapsed = 0f;
                while (elapsed < ghostFadeOutTime)
                {
                    elapsed += Time.deltaTime;
                    c.a = Mathf.Lerp(1f, 0f, elapsed / ghostFadeOutTime);
                    sr.color = c;
                    yield return null;
                }
            }

            Destroy(spawnedGhost);
        }

        hintActive = false;
    }

    /// <summary>
    /// Types out a string one character at a time into hintText.
    /// Player can press dismissKey during typing to instantly show the full text.
    /// </summary>
    IEnumerator TypeText(string fullMessage)
    {
        hintText.text = "";

        for (int i = 0; i < fullMessage.Length; i++)
        {
            hintText.text += fullMessage[i];

            // Play optional typing sound (skip for spaces)
            if (typeSFX != null && fullMessage[i] != ' ')
                AudioSource.PlayClipAtPoint(typeSFX, Camera.main.transform.position, typeSFXVolume);

            // Let the player skip ahead by pressing the dismiss key
            if (Input.GetKey(dismissKey))
            {
                hintText.text = fullMessage;
                yield break;
            }

            yield return new WaitForSeconds(typeSpeed);
        }
    }
}
