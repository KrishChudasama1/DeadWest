using UnityEngine;
using UnityEngine.UI;

public class FogEffect : MonoBehaviour
{
    [Header("Center Fog")]
    public float minAlpha = 40f;
    public float maxAlpha = 80f;
    public float pulseSpeed = 0.5f;

    [Header("Edge Fog")]
    public Image edgeFog;
    public float edgeMinAlpha = 120f;
    public float edgeMaxAlpha = 200f;
    public float edgePulseSpeed = 0.3f;

    private Image fogImage;
    private float timer;
    private float edgeTimer;

    void Start()
    {
        fogImage = GetComponent<Image>();
    }

    void Update()
    {
        timer += Time.deltaTime * pulseSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(timer, 1f));
        Color c = fogImage.color;
        c.a = alpha / 255f;
        fogImage.color = c;

        if (edgeFog != null)
        {
            edgeTimer += Time.deltaTime * edgePulseSpeed;
            float edgeAlpha = Mathf.Lerp(edgeMinAlpha, edgeMaxAlpha, Mathf.PingPong(edgeTimer, 1f));
            Color ec = edgeFog.color;
            ec.a = edgeAlpha / 255f;
            edgeFog.color = ec;
        }
    }
}