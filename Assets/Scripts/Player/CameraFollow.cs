using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("World Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -200f;
    [SerializeField] private float maxX =  200f;
    [SerializeField] private float minY = -100f;
    [SerializeField] private float maxY =  100f;

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

    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

  
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size   = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
