using UnityEngine;

public class GlitchEffect : MonoBehaviour
{
    private Vector3 originalPosition;
    private MeshRenderer[] renderers;

    [Header("Intenzita Glitche")]
    [Tooltip("Pravděpodobnost glitche v každém framu (např. 0.1 = 10%)")]
    public float glitchProbability = 0.1f;
    [Tooltip("Jak daleko se model může 'třást'")]
    public float positionJitterAmount = 0.05f;
    [Tooltip("Pravděpodobnost, že model na moment zmizí (problikne)")]
    public float invisibleProbability = 0.05f;

    void Start()
    {
        originalPosition = transform.localPosition;
        renderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        if (Random.value < glitchProbability)
        {
            // Position jitter
            transform.localPosition = originalPosition + Random.insideUnitSphere * positionJitterAmount;

            // Invisibility glitch
            bool visible = Random.value > invisibleProbability;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = visible;
            }
        }
        else
        {
            // Návrat k normálu
            transform.localPosition = originalPosition;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = true;
            }
        }
    }
}