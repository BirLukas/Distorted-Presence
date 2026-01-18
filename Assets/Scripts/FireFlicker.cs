using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    private Light fireLight;

    [Header("Nastavení blikání")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 5f;
    [Range(0.01f, 0.2f)]
    public float flickerSpeed = 0.05f;

    void Start()
    {
        fireLight = GetComponent<Light>();
    }

    void Update()
    {
        float targetIntensity = Random.Range(minIntensity, maxIntensity);
        fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetIntensity, flickerSpeed);
    }
}
