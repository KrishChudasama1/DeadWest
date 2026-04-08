using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

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

    private bool hasSnappedToTarget;

    private void OnEnable()
    {
        TryAcquireTarget();
    }
    private void Start() 
    {
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            
            transform.position = new Vector3(target.position.x, target.position.y, offset.z);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }

        if (target == null)
            TryAcquireTarget();

        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
                                      new Vector2(desiredPos.x, desiredPos.y));
        
        if (dist < deadzone) return;

        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

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
        hasSnappedToTarget = false;
    }

    private void TryAcquireTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
            return;

        target = player.transform;

        if (!hasSnappedToTarget)
        {
            transform.position = target.position + offset;
            hasSnappedToTarget = true;
        }
    }
}