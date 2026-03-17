using UnityEngine;

/// <summary>
/// Moves a tumbleweed across the screen with a slight bobbing motion.
/// Respawns on the opposite side when it goes off-screen.
/// Attach to a tumbleweed sprite for ambient desert atmosphere.
/// </summary>
public class Tumbleweed : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float respawnRangeX = 60f;

    private float baseY;
    private float timeOffset;

    private void Start()
    {
        baseY = transform.position.y;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
        speed += Random.Range(-0.5f, 0.5f);  // slight variation
    }

    private void Update()
    {
        // Roll across the desert
        float x = transform.position.x + speed * Time.deltaTime;
        float y = baseY + Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;

        transform.position = new Vector3(x, y, transform.position.z);
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

        // Respawn when off the far edge
        if (x > respawnRangeX)
        {
            x = -respawnRangeX;
            baseY = Random.Range(-8f, 8f);
            transform.position = new Vector3(x, baseY, transform.position.z);
        }
    }
}
