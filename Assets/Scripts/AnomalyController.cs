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
        AddedObject
    }

    [Header("Type")]
    public AnomalyType anomalyType;

    [Header("Settings")]
    public float transitionSpeed = 2f;

    [Header("Color Settings")]
    public Color targetColor = Color.red;

    [Header("Scale Settings")]
    public float scaleMultiplier = 1.5f;

    private bool isActive = false;
    public bool IsActive => isActive;

    private bool wasReported = false;
    public bool WasReported => wasReported;

    private Renderer[] renderers;
    private Color[] originalRendererColors;

    private Light[] lights;
    private Color[] originalLightColors;

    private Dictionary<Transform, Vector3> originalScales;

    void Start()
    {
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
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            originalScales[t] = t.localScale;
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