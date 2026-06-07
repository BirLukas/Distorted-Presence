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
    [Tooltip("Zadejte index materiálu, který se má změnit. -1 znamená všechny materiály.")]
    public int targetMaterialIndex = -1;

    [Header("Scale Settings")]
    public float scaleMultiplier = 1.5f;



    [Header("Visual Illusion Settings")]
    public float illusionJitterIntensity = 0.05f;
    public float illusionRotationSpeed = 50f;
    [Tooltip("Vzdálenost od středu obrazovky (0.5), kdy se začne projevovat iluze. Např. 0.3 znamená, že iluze funguje, když je X menší než 0.2 nebo větší než 0.8.")]
    public float peripheralThreshold = 0.3f;

    [Header("Audio Settings")]
    public AudioClip onReportSound;
    [Tooltip("Zvuk, který se bude přehrávat ve smyčce s pauzou (např. smích).")]
    public AudioClip intervalSound;
    [Tooltip("Pauza v sekundách mezi přehráními zvuku.")]
    public float delayBetweenSounds = 5f;
    private float soundTimer = 0f;
    private AudioSource audioSource;

    private GameObject currentShadowEffect;
    private bool isActive = false;
    public bool IsActive => isActive;

    private bool wasReported = false;
    public bool WasReported => wasReported;

    private Renderer[] renderers;
    private Material[][] rendererMaterials;
    private Color[][] originalRendererColors;

    private Light[] lights;
    private Color[] originalLightColors;

    private Dictionary<Transform, Vector3> originalScales;

    private Dictionary<Transform, Quaternion> originalRotations;
    private Dictionary<Transform, Vector3> originalPositions;
    private Dictionary<Collider, bool> originalCollidersEnabled;
    private Dictionary<Collider, bool> originalCollidersIsTrigger;
    void Start()
    {
        if (onReportSound != null || intervalSound != null)
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
        rendererMaterials = new Material[renderers.Length][];
        originalRendererColors = new Color[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            rendererMaterials[i] = renderers[i].materials;
            originalRendererColors[i] = new Color[rendererMaterials[i].Length];
            for (int m = 0; m < rendererMaterials[i].Length; m++)
            {
                Material mat = rendererMaterials[i][m];
                if (mat.HasProperty("_BaseColor"))
                    originalRendererColors[i][m] = mat.GetColor("_BaseColor");
                else if (mat.HasProperty("_Color"))
                    originalRendererColors[i][m] = mat.color;
                else
                    originalRendererColors[i][m] = Color.white;
            }
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

        originalCollidersEnabled = new Dictionary<Collider, bool>();
        originalCollidersIsTrigger = new Dictionary<Collider, bool>();
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
        {
            originalCollidersEnabled[c] = c.enabled;
            originalCollidersIsTrigger[c] = c.isTrigger;
        }

        // Nastavit do výchozího stavu hned při startu, pokud zrovna nebyla aktivována (skryje AddedObject a ShadowChange)
        if (!isActive)
        {
            ResetAnomaly();
        }
    }

    void Update()
    {
        if (!isActive) return;

        if (intervalSound != null && audioSource != null)
        {
            if (!audioSource.isPlaying)
            {
                soundTimer -= Time.deltaTime;
                if (soundTimer <= 0f)
                {
                    audioSource.clip = intervalSound;
                    audioSource.Play();
                    soundTimer = delayBetweenSounds;
                }
            }
        }

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
        for (int i = 0; i < rendererMaterials.Length; i++)
        {
            for (int m = 0; m < rendererMaterials[i].Length; m++)
            {
                if (targetMaterialIndex != -1 && m != targetMaterialIndex) continue;

                Material mat = rendererMaterials[i][m];
                Color currentColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                Color newColor = Color.Lerp(
                    currentColor,
                    targetColor,
                    Time.deltaTime * transitionSpeed
                );

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", newColor);
                
                if (mat.HasProperty("_Color"))
                    mat.color = newColor;
            }
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

        if (audioSource != null && audioSource.isPlaying && audioSource.clip == intervalSound)
        {
            audioSource.Stop();
        }

        if (onReportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(onReportSound);
        }
    }

    public void Activate()
    {
        isActive = true;
        soundTimer = 0f;

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
            gameObject.SetActive(true);
            
            foreach (Renderer r in renderers)
            {
                if (r != null)
                {
                    r.enabled = true;
                    // Objekt sám o sobě už nebude vrhat další stín
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            
            // Nastavíme kolize jako Trigger, aby hráč mohl "stínem" projít, ale foťák (raycast) ho trefil
            foreach (Collider c in GetComponentsInChildren<Collider>(true))
            {
                if (c != null)
                {
                    c.enabled = true;
                    c.isTrigger = true;
                }
            }

            ApplyShadowEffects();
        }
    }
    void ApplyShadowEffects()
    {
        if (currentShadowEffect != null)
        {
            Destroy(currentShadowEffect);
        }

        currentShadowEffect = new GameObject("ShadowAnomaly_VFX");
        currentShadowEffect.transform.SetParent(transform, false);
        // Zvedneme trochu nahoru, aby světlo a částice šly ze středu postavy (předpokládáme pivot dole)
        currentShadowEffect.transform.localPosition = new Vector3(0, 1f, 0);

        // 1. GLOWING EFEKT (světlo)
        Color[] glowColors = new Color[]
        {
            new Color(0.7f, 0f, 1f),   // Fialová
            new Color(0f, 1f, 0.8f),   // Teal / Modrozelená
            new Color(1f, 0.2f, 0.2f)  // Červená
        };
        Color selectedColor = glowColors[Random.Range(0, glowColors.Length)];

        Light glowLight = currentShadowEffect.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.range = 1f;
        glowLight.intensity = 0.5f; // Slabý glow efekt do okolí
        glowLight.color = selectedColor;

        // 2. ČÁSTICE (Particles)
        ParticleSystem ps = currentShadowEffect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Částice už nemění barvu a jsou hodně slabé/průhledné (např. slabě černá/šedá "stínová" barva)
        main.startColor = new Color(0.1f, 0.1f, 0.1f, 0.15f);

        // 2 různé typy particles ze kterých se vybírá
        int particleType = Random.Range(0, 2);
        
        var emission = ps.emission;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1.2f, 1.8f, 1.2f);

        if (particleType == 0) // Typ 1: Velmi jemný stoupající prach
        {
            main.startLifetime = 2.5f;
            main.startSpeed = 0.3f;
            main.startSize = 0.08f;
            main.gravityModifier = -0.02f;
            emission.rateOverTime = 15; // Méně výrazné, méně částic
        }
        else // Typ 2: Pomalá, lehce poletující stínová mlha/aura
        {
            main.startLifetime = 3f;
            main.startSpeed = 0.05f;
            main.startSize = 0.5f;
            main.gravityModifier = 0f;
            emission.rateOverTime = 8; // Málo částic, aby to nebylo moc výrazné
            
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.3f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        }

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        // Shader, který funguje vždy a podporuje průhlednost a barvy částic
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        ps.Play();
    }

    public void ResetAnomaly()
    {
        isActive = false;

        if (currentShadowEffect != null)
        {
            Destroy(currentShadowEffect);
            currentShadowEffect = null;
        }

        if (audioSource != null && audioSource.isPlaying && audioSource.clip == intervalSound)
        {
            audioSource.Stop();
        }
        soundTimer = 0f;

        foreach (Renderer r in renderers) r.enabled = true;
        foreach (Light l in lights) l.enabled = true;

        for (int i = 0; i < rendererMaterials.Length; i++)
        {
            for (int m = 0; m < rendererMaterials[i].Length; m++)
            {
                Material mat = rendererMaterials[i][m];
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", originalRendererColors[i][m]);
                    
                if (mat.HasProperty("_Color"))
                    mat.color = originalRendererColors[i][m];
            }
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

        if (originalCollidersEnabled != null)
        {
            foreach (var kvp in originalCollidersEnabled)
            {
                if (kvp.Key != null) kvp.Key.enabled = kvp.Value;
            }
        }
        
        if (originalCollidersIsTrigger != null)
        {
            foreach (var kvp in originalCollidersIsTrigger)
            {
                if (kvp.Key != null) kvp.Key.isTrigger = kvp.Value;
            }
        }

        if (anomalyType == AnomalyType.MissingObject)
        {
            gameObject.SetActive(true);
        }
        else if (anomalyType == AnomalyType.AddedObject || anomalyType == AnomalyType.ShadowChange)
        {
            gameObject.SetActive(false);
        }
    }
}