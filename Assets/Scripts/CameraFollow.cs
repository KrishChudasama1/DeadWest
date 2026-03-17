using UnityEngine;

/// <summary>
/// Smooth-follow camera for 2D top-down view.
/// Attach to Main Camera. Assign the Player transform as target.
/// Optional: define world bounds so the camera never shows outside the map.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("World Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX =  50f;
    [SerializeField] private float minY = -50f;
    [SerializeField] private float maxY =  50f;

    [Header("Deadzone")]
    [Tooltip("Camera won't move until the player exceeds this distance from center.")]
    [SerializeField] private float deadzone = 0.5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        // Deadzone check
        float dist = Vector2.Distance(transform.position, desiredPos + Vector3.forward * 10f);
        if (dist < deadzone) return;

        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // Clamp to world bounds
        if (useBounds)
        {
            float camHeight = Camera.main.orthographicSize;
            float camWidth  = camHeight * Camera.main.aspect;

            smoothed.x = Mathf.Clamp(smoothed.x, minX + camWidth,  maxX - camWidth);
            smoothed.y = Mathf.Clamp(smoothed.y, minY + camHeight, maxY - camHeight);
        }

        transform.position = smoothed;
    }

    /// <summary>
    /// Call this to set the target at runtime (e.g., after spawning the player).
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Draw bounds in the editor for easy debugging
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size   = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
