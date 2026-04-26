using UnityEngine;

[RequireComponent(typeof(Light))]
public class BlinkingLight : MonoBehaviour
{
    private Light targetLight;
    private float baseIntensity;
    private float noiseOffset;

    [Header("Blink Settings (Outages)")]
    [Tooltip("Intenzita světla když je 'vypnuto' (blikne)")]
    public float minIntensity = 0f;
    public float minOffTime = 0.05f;
    public float maxOffTime = 0.2f;
    public float minOnTime = 0.5f;
    public float maxOnTime = 3f;

    [Header("Flicker Settings (Subtle Jitter)")]
    [Tooltip("Amplituda chvění (0 = žádné, 1 = max)")]
    [Range(0f, 1f)]
    public float flickerAmount = 0.07f;
    [Tooltip("Rychlost chvění")]
    public float flickerSpeed = 15f;
    
    [Header("Transitions")]
    [Tooltip("Plynulost přechodu mezi stavy (vyšší číslo = plynulejší)")]
    public float smoothing = 20f;

    private float timer;
    private bool isOff = false;
    private float currentIntensity;

    void Start()
    {
        targetLight = GetComponent<Light>();
        if (targetLight == null) return;

        baseIntensity = targetLight.intensity;
        currentIntensity = baseIntensity;
        timer = Random.Range(minOnTime, maxOnTime);
        noiseOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        if (targetLight == null) return;

        timer -= Time.deltaTime;
        
        if (timer <= 0)
        {
            isOff = !isOff;
            if (isOff)
                timer = Random.Range(minOffTime, maxOffTime);
            else
                timer = Random.Range(minOnTime, maxOnTime);
        }

        // Výpočet cílové intenzity
        float target;
        if (isOff)
        {
            target = minIntensity;
        }
        else
        {
            // Perlinův šum vytváří přirozenější "organické" chvění než Random.Range
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
            float jitter = (noise - 0.5f) * 2f * flickerAmount * baseIntensity;
            target = baseIntensity + jitter;
        }

        // Plynulý přechod zabrání "stroboskopickému" efektu a vypadá víc jako skutečná žárovka
        currentIntensity = Mathf.Lerp(currentIntensity, target, Time.deltaTime * smoothing);
        targetLight.intensity = currentIntensity;
    }
}