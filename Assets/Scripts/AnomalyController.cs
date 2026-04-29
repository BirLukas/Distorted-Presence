using UnityEngine;
using System.Collections.Generic;

public class AnomalyController : MonoBehaviour
{
    public enum AnomalyType
    {
        ColorChange,
        ScaleChange,
        LightColorChange,
        MissingObject,
        AddedObject,
        ShadowChange,
        VisualIllusion
    }

    [Header("Type")]
    public AnomalyType anomalyType;

    [Header("Settings")]
    public float transitionSpeed = 2f;

    [Header("Color Settings")]
    public Color targetColor = Color.red;

    [Header("Scale Settings")]
    public float scaleMultiplier = 1.5f;

    [Header("Shadow Settings")]
    public UnityEngine.Rendering.ShadowCastingMode targetShadowMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

    [Header("Visual Illusion Settings")]
    public float illusionJitterIntensity = 0.05f;
    public float illusionRotationSpeed = 50f;
    [Tooltip("Vzdálenost od středu obrazovky (0.5), kdy se začne projevovat iluze. Např. 0.3 znamená, že iluze funguje, když je X menší než 0.2 nebo větší než 0.8.")]
    public float peripheralThreshold = 0.3f;

    [Header("Audio Settings")]
    public AudioClip onReportSound;
    private AudioSource audioSource;

    private bool isActive = false;
    public bool IsActive => isActive;

    private bool wasReported = false;
    public bool WasReported => wasReported;

    private Renderer[] renderers;
    private Color[] originalRendererColors;

    private Light[] lights;
    private Color[] originalLightColors;

    private Dictionary<Transform, Vector3> originalScales;
    private UnityEngine.Rendering.ShadowCastingMode[] originalShadowModes;
    private Dictionary<Transform, Quaternion> originalRotations;
    private Dictionary<Transform, Vector3> originalPositions;

    void Start()
    {
        if (onReportSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // Optional: make it 2D or 3D sound
                audioSource.spatialBlend = 1f; 
            }
        }

        renderers = GetComponentsInChildren<Renderer>(true);
        originalRendererColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalRendererColors[i] = renderers[i].material.color;
        }

        lights = GetComponentsInChildren<Light>(true);
        originalLightColors = new Color[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            originalLightColors[i] = lights[i].color;
        }

        originalScales = new Dictionary<Transform, Vector3>();
        originalRotations = new Dictionary<Transform, Quaternion>();
        originalPositions = new Dictionary<Transform, Vector3>();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            originalScales[t] = t.localScale;
            originalRotations[t] = t.localRotation;
            originalPositions[t] = t.localPosition;
        }

        originalShadowModes = new UnityEngine.Rendering.ShadowCastingMode[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalShadowModes[i] = renderers[i].shadowCastingMode;
        }
    }

    void Update()
    {
        if (!isActive) return;

        switch (anomalyType)
        {
            case AnomalyType.ColorChange:
                ApplyColorChange();
                break;

            case AnomalyType.ScaleChange:
                ApplyScaleChange();
                break;

            case AnomalyType.LightColorChange:
                ApplyLightColorChange();
                break;

            case AnomalyType.VisualIllusion:
                ApplyVisualIllusion();
                break;
        }
    }

    void ApplyVisualIllusion()
    {
        if (Camera.main == null) return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        // Zkontrolujeme, zda je objekt před kamerou (z > 0) a v rámci viewportu obrazovky
        bool isVisible = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

        // Vzdálenost od středu obrazovky (0.5) v ose X
        float distanceFromCenter = Mathf.Abs(viewportPos.x - 0.5f);

        if (isVisible && distanceFromCenter > peripheralThreshold)
        {
            // Je v periferním vidění - aplikujeme jitter a rotaci
            foreach (var kvp in originalPositions)
            {
                Transform t = kvp.Key;
                if (t == null) continue;
                Vector3 originalPos = kvp.Value;
                Vector3 jitter = new Vector3(
                    Random.Range(-illusionJitterIntensity, illusionJitterIntensity),
                    Random.Range(-illusionJitterIntensity, illusionJitterIntensity),
                    Random.Range(-illusionJitterIntensity, illusionJitterIntensity)
                );
                t.localPosition = originalPos + jitter;
            }

            foreach (var kvp in originalRotations)
            {
                Transform t = kvp.Key;
                if (t == null) continue;
                t.Rotate(Vector3.up * illusionRotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Je sledován přímo (nebo není vidět) - ihned vrátit do původního stavu
            foreach (var kvp in originalPositions)
            {
                if (kvp.Key != null) kvp.Key.localPosition = kvp.Value;
            }
            foreach (var kvp in originalRotations)
            {
                if (kvp.Key != null) kvp.Key.localRotation = kvp.Value;
            }
        }
    }

    void ApplyColorChange()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = Color.Lerp(
                renderers[i].material.color,
                targetColor,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    void ApplyScaleChange()
    {
        foreach (var kvp in originalScales)
        {
            Transform t = kvp.Key;

            Vector3 original = kvp.Value;

            t.localScale = Vector3.Lerp(
                t.localScale,
                original * scaleMultiplier,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    void ApplyLightColorChange()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].color = Color.Lerp(
                lights[i].color,
                targetColor,
                Time.deltaTime * transitionSpeed
            );
        }
    }
    public void ReportAnomaly()
    {
        isActive = false;
        wasReported = true;

        if (onReportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(onReportSound);
        }
    }

    public void Activate()
    {
        isActive = true;

        if (renderers == null) renderers = GetComponentsInChildren<Renderer>(true);
        if (lights == null) lights = GetComponentsInChildren<Light>(true);

        if (anomalyType == AnomalyType.MissingObject)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null) r.enabled = false;
            }
            foreach (Light l in lights)
            {
                if (l != null) l.enabled = false;
            }
        }
        else if (anomalyType == AnomalyType.AddedObject)
        {
            gameObject.SetActive(true);
            foreach (Renderer r in renderers)
            {
                if (r != null) r.enabled = true;
            }
            foreach (Light l in lights)
            {
                if (l != null) l.enabled = true;
            }
        }
        else if (anomalyType == AnomalyType.ShadowChange)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null) r.shadowCastingMode = targetShadowMode;
            }
        }
    }
    public void ResetAnomaly()
    {
        isActive = false;

        foreach (Renderer r in renderers) r.enabled = true;
        foreach (Light l in lights) l.enabled = true;

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalRendererColors[i];
        }

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].color = originalLightColors[i];
        }

        foreach (var kvp in originalScales)
        {
            if (kvp.Key != null) kvp.Key.localScale = kvp.Value;
        }

        foreach (var kvp in originalPositions)
        {
            if (kvp.Key != null) kvp.Key.localPosition = kvp.Value;
        }

        foreach (var kvp in originalRotations)
        {
            if (kvp.Key != null) kvp.Key.localRotation = kvp.Value;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].shadowCastingMode = originalShadowModes[i];
        }

        if (anomalyType == AnomalyType.MissingObject)
        {
            gameObject.SetActive(true);
        }
        else if (anomalyType == AnomalyType.AddedObject)
        {
            gameObject.SetActive(false);
        }
    }
}