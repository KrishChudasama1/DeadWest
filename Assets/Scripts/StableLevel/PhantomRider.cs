using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Moves a horse+rider sprite along a waypoint-based oval/circular track.
/// The rider loops continuously and can be hit by lasso projectiles.
/// After 3 hits, the rider is dismounted and the chase phase ends.
/// </summary>
public class PhantomRider : MonoBehaviour
{
    [Header("Waypoint Path")]
    [Tooltip("Ordered list of waypoint transforms that define the oval track.")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("Movement")]
    [Tooltip("Base movement speed along the track.")]
    [SerializeField] private float baseSpeed = 4f;

    [Tooltip("Speed increase per lasso hit.")]
    [SerializeField] private float speedIncreasePerHit = 0.8f;

    [Header("Lasso Hits")]
    [Tooltip("Number of lasso hits required to dismount the rider.")]
    [SerializeField] private int hitsRequired = 3;

    [Header("Events")]
    [Tooltip("Fired when the rider is fully dismounted after all required hits.")]
    public UnityEvent OnDismounted;

    [Tooltip("Reference to the StableLevelManager to advance phases.")]
    [SerializeField] private StableLevelManager levelManager;

    private int _currentHits;
    private int _currentWaypointIndex;
    private float _currentSpeed;
    private bool _isDismounted;
    private bool _isActive;
    private SpriteRenderer _sr;
    private Animator _animator;

    /// <summary>
    /// Returns true if the rider has been dismounted.
    /// </summary>
    public bool IsDismounted => _isDismounted;

    /// <summary>
    /// Returns the current number of lasso hits received.
    /// </summary>
    public int CurrentHits => _currentHits;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _currentSpeed = baseSpeed;
        gameObject.tag = "Enemy";
    }

    /// <summary>
    /// Activates the rider to begin moving along the track.
    /// </summary>
    public void Activate()
    {
        _isActive = true;
        _currentHits = 0;
        _currentSpeed = baseSpeed;
        _isDismounted = false;
        _currentWaypointIndex = 0;
    }

    /// <summary>
    /// Deactivates the rider, stopping all movement.
    /// </summary>
    public void Deactivate()
    {
        _isActive = false;
    }

    private void Update()
    {
        if (!_isActive || _isDismounted || waypoints.Count == 0) return;

        Transform target = waypoints[_currentWaypointIndex];
        Vector2 direction = (target.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            _currentSpeed * Time.deltaTime
        );

        // Flip sprite based on movement direction
        if (_sr != null && direction.x != 0)
            _sr.flipX = direction.x < 0;

        // Advance to next waypoint when close enough
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
        }
    }

    /// <summary>
    /// Called by LassoProjectile when a lasso hits this rider.
    /// Flashes the rider red, increases speed, and checks for dismount.
    /// </summary>
    public void RegisterLassoHit()
    {
        if (_isDismounted) return;

        _currentHits++;
        _currentSpeed += speedIncreasePerHit;

        Debug.Log($"[PhantomRider] Hit {_currentHits}/{hitsRequired}! Speed now: {_currentSpeed}");

        StartCoroutine(FlashRed());

        if (_currentHits >= hitsRequired)
        {
            StartCoroutine(DismountRoutine());
        }
    }

    /// <summary>
    /// Resets the hit counter and repositions the rider at the first waypoint.
    /// Called when the player takes damage during the chase phase.
    /// </summary>
    public void ResetProgress()
    {
        _currentHits = 0;
        _currentSpeed = baseSpeed;

        if (waypoints.Count > 0)
        {
            _currentWaypointIndex = 0;
            transform.position = waypoints[0].position;
        }

        Debug.Log("[PhantomRider] Progress reset!");
    }

    private IEnumerator FlashRed()
    {
        if (_sr != null)
        {
            _sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            _sr.color = Color.white;
        }
    }

    private IEnumerator DismountRoutine()
    {
        _isDismounted = true;
        _isActive = false;

        Debug.Log("[PhantomRider] Dismounted!");

        // Brief pause before triggering next phase
        yield return new WaitForSeconds(0.5f);

        OnDismounted?.Invoke();

        // Advance to Phase 5
        if (levelManager != null)
            levelManager.AdvancePhase();
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            int next = (i + 1) % waypoints.Count;
            if (waypoints[next] == null) continue;
            Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);
        }
    }
}
