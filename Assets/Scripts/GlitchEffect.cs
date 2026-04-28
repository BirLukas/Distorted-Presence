using UnityEngine;

public class GlitchEffect : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private MeshRenderer[] renderers;

    [Header("Nastavení Sanity")]
    [Tooltip("Pokud je true, efekt se řídí aktuální sanitou hráče.")]
    public bool useSanity = true;
    [Tooltip("Hranice sanity, pod kterou začíná glitch (např. 60)")]
    public float sanityThreshold = 60f;

    [Header("Intenzita Glitche (při 0% sanitě)")]
    [Tooltip("Pravděpodobnost glitche v každém framu")]
    public float maxGlitchProbability = 0.3f;
    [Tooltip("Jak daleko se objekt může 'třást'")]
    public float maxPositionJitterAmount = 0.05f;
    [Tooltip("Maximální rotace při glitchi (vhodné pro kameru)")]
    public float maxRotationJitterAmount = 1.5f;
    [Tooltip("Pravděpodobnost, že objekt na moment zmizí")]
    public float maxInvisibleProbability = 0.05f;

    [Header("Vyhlazování")]
    [Tooltip("Rychlost návratu do původní polohy")]
    public float returnSpeed = 10f;

    private Vector3 currentPositionJitter;
    private Quaternion currentRotationJitter = Quaternion.identity;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        renderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        float intensity = 0f;

        if (useSanity && SanityManager.Instance != null)
        {
            float currentSanity = SanityManager.Instance.CurrentSanity;
            if (currentSanity < sanityThreshold)
            {
                intensity = 1f - (currentSanity / sanityThreshold);
            }
        }
        else if (!useSanity)
        {
            intensity = 1f;
        }

        if (intensity > 0 && Random.value < (maxGlitchProbability * intensity))
        {
            // Calculate jitter
            currentPositionJitter = Random.insideUnitSphere * (maxPositionJitterAmount * intensity);
            
            float rotX = Random.Range(-1f, 1f) * maxRotationJitterAmount * intensity;
            float rotY = Random.Range(-1f, 1f) * maxRotationJitterAmount * intensity;
            float rotZ = Random.Range(-1f, 1f) * maxRotationJitterAmount * intensity;
            currentRotationJitter = Quaternion.Euler(rotX, rotY, rotZ);

            // Invisibility glitch
            if (renderers.Length > 0)
            {
                bool visible = Random.value > (maxInvisibleProbability * intensity);
                foreach (var r in renderers)
                {
                    if (r != null) r.enabled = visible;
                }
            }
        }
        else
        {
            // Smooth return
            currentPositionJitter = Vector3.Lerp(currentPositionJitter, Vector3.zero, Time.deltaTime * returnSpeed);
            currentRotationJitter = Quaternion.Lerp(currentRotationJitter, Quaternion.identity, Time.deltaTime * returnSpeed);

            if (renderers.Length > 0)
            {
                foreach (var r in renderers)
                {
                    if (r != null && !r.enabled) r.enabled = true;
                }
            }
        }

        // Apply jitter only if NOT a camera handled by MouseLook (or just apply it here for objects)
        // For camera, MouseLook will read these values.
        if (GetComponent<Camera>() == null)
        {
            transform.localPosition = originalPosition + currentPositionJitter;
            transform.localRotation = originalRotation * currentRotationJitter;
        }
    }

    public Vector3 GetPositionOffset() => currentPositionJitter;
    public Quaternion GetRotationOffset() => currentRotationJitter;
}