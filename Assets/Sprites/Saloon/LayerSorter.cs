using UnityEngine;

public class DepthSorter : MonoBehaviour
{
    private SpriteRenderer sr;
    private Collider2D col;

    [Header("Settings")]
    public int sortingOffset = 0; // tweak this in Inspector if needed

    private void Start()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        if (sr == null) return;

        float bottom = col != null
            ? col.bounds.min.y
            : transform.position.y - sr.bounds.extents.y;

        sr.sortingOrder = Mathf.RoundToInt(-bottom * 100) + sortingOffset;
    }
}