using UnityEngine;

[RequireComponent(typeof(Light))]
public class BlinkingLight : MonoBehaviour
{
    private Light targetLight;
    private float baseIntensity;

    [Header("Blink Settings")]
    [Tooltip("Intenzita světla když je 'vypnuto' (blikne)")]
    public float minIntensity = 0f;
    public float minOffTime = 0.05f;
    public float maxOffTime = 0.2f;
    public float minOnTime = 0.1f;
    public float maxOnTime = 1f;

    private float timer;
    private bool isOff = false;

    void Start()
    {
        targetLight = GetComponent<Light>();
        baseIntensity = targetLight.intensity;
        timer = Random.Range(minOnTime, maxOnTime);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            isOff = !isOff;
            if (isOff)
            {
                targetLight.intensity = minIntensity;
                timer = Random.Range(minOffTime, maxOffTime);
            }
            else
            {
                targetLight.intensity = baseIntensity;
                timer = Random.Range(minOnTime, maxOnTime);
            }
        }
    }
}