using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DevPresentationTools : MonoBehaviour
{
    [Header("Cheats Settings")]
    [Tooltip("Stiskni klávesu N pro vynucení stínové anomálie.")]
    public Key spawnShadowAnomalyKey = Key.N;

    [Tooltip("Stiskni klávesu M pro přeskočení rovnou do konce (Ending Scene) s fiktivními daty.")]
    public Key jumpToEndingKey = Key.M;

    private void Awake()
    {
        // Nechceme, aby se zničil při načítání nových scén, ale stačí nám i v MainScene.
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Vynucení spawnu stínové anomálie
        if (Keyboard.current[spawnShadowAnomalyKey].wasPressedThisFrame)
        {
            SpawnShadowAnomaly();
        }

        // Skočení rovnou do konce s falešnými daty (pro ukázku EndingScene)
        if (Keyboard.current[jumpToEndingKey].wasPressedThisFrame)
        {
            JumpToEndingWithFakeData();
        }
    }

    private void SpawnShadowAnomaly()
    {
        // Najdeme VŠECHNY anomálie ve scéně (i ty neaktivní), nehledě na to, jaký je den.
        AnomalyController[] allAnomalies = FindObjectsByType<AnomalyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AnomalyController targetAnomaly = null;

        foreach (var anomaly in allAnomalies)
        {
            string lowerName = anomaly.gameObject.name.ToLower();
            if (lowerName.Contains("shadow") || lowerName.Contains("stin"))
            {
                targetAnomaly = anomaly;
                break;
            }
        }

        // Pokud jsme stále nenašli, upozorníme
        if (targetAnomaly == null)
        {
            Debug.LogError("[DEV CHEAT] Stínová anomálie se nenašla! Ujisti se, že má v názvu slovo 'shadow' nebo 'stin'.");
            
            // Fallback na jakoukoliv volnou anomálii z dnešního dne
            if (AnomalyManager.Instance != null && AnomalyManager.Instance.anomalies.Count > 0)
            {
                targetAnomaly = AnomalyManager.Instance.anomalies[0];
            }
        }

        if (targetAnomaly != null && !targetAnomaly.IsActive)
        {
            targetAnomaly.Activate();
            
            // Přidáme ji manuálně do seznamu aktivovaných, aby ji systém rozeznal pro postih/skóre.
            if (GameProgressionManager.Instance != null && !GameProgressionManager.Instance.activatedAnomalyNames.Contains(targetAnomaly.gameObject.name))
            {
                GameProgressionManager.Instance.activatedAnomalyNames.Add(targetAnomaly.gameObject.name);
            }
            
            Debug.Log($"[DEV CHEAT] Vynuceně aktivována anomálie: {targetAnomaly.gameObject.name}");
        }
        else if (targetAnomaly != null)
        {
            Debug.Log($"[DEV CHEAT] Anomálie {targetAnomaly.gameObject.name} je již aktivní.");
        }
    }

    private void JumpToEndingWithFakeData()
    {
        Debug.Log("[DEV CHEAT] Přeskakuji rovnou do Ending Scene s fiktivními daty!");

        if (GameProgressionManager.Instance != null)
        {
            // Nastavíme falešná data (např. 8 nalezených z 10)
            GameProgressionManager.Instance.globalPhotographed = 8;
            GameProgressionManager.Instance.globalTriggered = 10;

            // NEMAŽEME předchozí fotky, abys v ukázce viděl ty, které jsi reálně během dema vyfotil!
            // GameProgressionManager.Instance.globalPhotos.Clear();

            int w = Screen.width;
            int h = Screen.height;

            // Pokud jsi nestihl vyfotit aspoň 2 fotky, přidáme nějaké černé/tmavé fiktivní fotky, 
            // aby tam nebyla ta hnusná modrá barva (minule se ti vygenerovala Color.blue, což vypadá jako Unity pozadí!)
            if (GameProgressionManager.Instance.globalPhotos.Count < 2)
            {
                Color[] colors = new Color[] { Color.black, new Color(0.1f, 0.1f, 0.1f) }; // Tmavé barvy
                foreach(var col in colors)
                {
                    Texture2D fakePhoto = new Texture2D(w, h);
                    Color[] pixels = new Color[w * h];
                    for(int i = 0; i < pixels.Length; i++) pixels[i] = col;
                    fakePhoto.SetPixels(pixels);
                    fakePhoto.Apply();
                    
                    GameProgressionManager.Instance.globalPhotos.Add(fakePhoto);
                }
            }
        }

        // Odjistíme kurzor (normálně se o to stará EndingSceneController, ale pro jistotu)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Odpauzujeme hru, kdyby byla pauznutá
        Time.timeScale = 1f;

        // Načteme závěrečnou scénu. 
        // DŮLEŽITÉ: Musí se jmenovat tak, jak ji máš pojmenovanou v Build Settings (zřejmě "EndingScene" podle kódu EndingSceneController).
        SceneManager.LoadScene("EndingScene"); // Uprav název, pokud se scéna jmenuje jinak!
    }
}
