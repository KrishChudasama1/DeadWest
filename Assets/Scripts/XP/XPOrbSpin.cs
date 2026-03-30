using UnityEngine;

public class XPOrbSpin : MonoBehaviour
{
    public float jitterAmount = 0.05f;
    public float jitterSpeed = 10f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float offsetX = Mathf.Sin(Time.time * jitterSpeed) * jitterAmount;
        float offsetY = Mathf.Cos(Time.time * jitterSpeed * 1.3f) * jitterAmount;
        transform.position = startPos + new Vector3(offsetX, offsetY, 0);
    }
}