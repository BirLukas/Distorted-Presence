/*using UnityEngine;
using System.Collections.Generic;

public class AnomalyController : MonoBehaviour
{
    public enum AnomalyType
    {
        ColorChange,
        ScaleChange,
        LightColorChange
    }

    public AnomalyType anomalyType;
    public float transitionSpeed = 2f;
    public float scaleMultiplier = 2f;
    public Color targetColor = Color.red;

    private bool isActive = false;
    public bool IsActive => isActive;

    private Dictionary<Transform, Vector3> originalScales;

    void Start()
    {
        Debug.Log("▶ [AnomalyController] START na objektu: " + gameObject.name);

        originalScales = new Dictionary<Transform, Vector3>();

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            originalScales[t] = t.localScale;
            Debug.Log("   → Uložen scale childu: " + t.name + " scale = " + t.localScale);
        }
    }

    public void Activate()
    {
        Debug.Log("▶ [AnomalyController] Aktivováno: " + gameObject.name);
        isActive = true;
    }

    void Update()
    {
        if (!isActive) return;

        if (anomalyType == AnomalyType.ScaleChange)
        {
            Debug.Log("⟶ SCALE tick na objektu: " + gameObject.name);

            foreach (var kvp in originalScales)
            {
                Transform t = kvp.Key;
                Vector3 orig = kvp.Value;
                Vector3 target = orig * scaleMultiplier;

                Debug.Log("   • Měním scale: " + t.name +
                          " z " + t.localScale + " → " + target);

                t.localScale = Vector3.Lerp(t.localScale, target, Time.deltaTime * 5f);
            }
        }
    }
}
*/

using UnityEngine;
using System.Collections.Generic;

public class AnomalyController : MonoBehaviour
{
    public enum AnomalyType
    {
        ColorChange,        // mění barvu rendererů (meshů)
        ScaleChange,        // mění scale všech child transformů
        LightColorChange    // mění barvu všech Light komponent
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

    // Renderery (mesh)
    private Renderer[] renderers;
    private Color[] originalRendererColors;

    // Světla
    private Light[] lights;
    private Color[] originalLightColors;

    // Scale všech transformů pod tímto objektem
    private Dictionary<Transform, Vector3> originalScales;

    void Start()
    {
        // Najdi všechny rendery (koberec, potion, mesh v child objektu atd.)
        renderers = GetComponentsInChildren<Renderer>(true);
        originalRendererColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalRendererColors[i] = renderers[i].material.color;
        }

        // Najdi všechny Light komponenty (pokud je to anomálie světla)
        lights = GetComponentsInChildren<Light>(true);
        originalLightColors = new Color[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            originalLightColors[i] = lights[i].color;
        }

        // Uložíme původní scale všech Transformů pod tímto objektem
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

            // POVOLENO zvětšit i root (t != transform podmínka pryč)
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

    public void Activate()
    {
        isActive = true;
    }

    // Kdyby ses někdy rozhodl anomálie resetovat:
    public void ResetAnomaly()
    {
        isActive = false;

        // barvy meshů
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalRendererColors[i];
        }

        // barvy světel
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].color = originalLightColors[i];
        }

        // scale
        foreach (var kvp in originalScales)
        {
            kvp.Key.localScale = kvp.Value;
        }
    }
}