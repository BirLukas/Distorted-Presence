using UnityEngine;

public class PhotoCapture : MonoBehaviour
{
    [Header("Capture Settings")]
    public float raycastDistance = 15f;
    public LayerMask anomalyLayer;

    [Header("Ammo Settings (Leden)")]
    public int maxFilmCount = 10;
    private int currentFilmCount;

    [Header("Camera Effect Settings")]
    public AudioSource cameraShutterSound;

    void Start()
    {
        currentFilmCount = maxFilmCount;
        UpdateFilmUI();
    }

    /// <summary>
    /// Spustí detekci anomálie, odeète snímek a upraví Sanity.
    /// Voláno ze skriptu CameraSystem.
    /// </summary>
    public void TryCaptureAnomaly()
    {
        // 1. KONTROLA MUNICE
        if (currentFilmCount <= 0)
        {
            Debug.Log("Došly snímky! Už nemùžeš fotit.");
            return;
        }

        // --- Snímek je odebrán ---
        currentFilmCount--;
        UpdateFilmUI();

        if (cameraShutterSound != null)
        {
            cameraShutterSound.Play();
        }

        // 2. RAYCAST a ZPRACOVÁNÍ
        // Raycast se støílí z pozice tohoto skriptu (Main Camera)
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDistance, anomalyLayer))
        {
            AnomalyController anomaly = hit.collider.GetComponentInParent<AnomalyController>();

            if (anomaly != null)
            {
                ProcessAnomalyCapture(anomaly);
            }
            else
            {
                // Penalizace za focení nìèeho, co má sice Layer, ale není to anomálie (špatná fotka)
                ApplyIncorrectCapturePenalty();
            }
        }
        else
        {
            // Penalizace za focení do prázdna/zdi
            ApplyIncorrectCapturePenalty();
        }
    }

    private void ProcessAnomalyCapture(AnomalyController anomaly)
    {
        if (SanityManager.Instance == null) return;

        if (anomaly.IsActive)
        {
            // SPRÁVNÉ NAHLÁŠENÍ (Prosinec - Bonus)
            SanityManager.Instance.AddSanity(SanityManager.Instance.correctAnomalyBonus);

            // Oznaèení anomálie jako vyøešené
            anomaly.ResetAnomaly();

            Debug.Log($"Anomálie {anomaly.gameObject.name} nahlášena. Sanity + {SanityManager.Instance.correctAnomalyBonus}%");
        }
        else
        {
            // ŠPATNÉ NAHLÁŠENÍ (Fotíš neaktivní/resetovanou anomálii)
            ApplyIncorrectCapturePenalty();

            Debug.LogWarning($"Špatná fotka! Objekt {anomaly.gameObject.name} už není anomálie.");
        }
    }

    private void ApplyIncorrectCapturePenalty()
    {
        // Odeètení sanity za špatné nahlášení (Prosinec - Penalizace)
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.RemoveSanity(SanityManager.Instance.incorrectAnomalyPenalty);
            Debug.Log($"Špatná fotka. Sanity - {SanityManager.Instance.incorrectAnomalyPenalty}%");
        }
    }

    // Pro další den (Leden)
    public void RefillFilm()
    {
        currentFilmCount = maxFilmCount;
        UpdateFilmUI();
    }

    // Pro Anomaly Manager (Leden)
    public int GetRemainingFilmCount()
    {
        return currentFilmCount;
    }

    // Pro UI
    private void UpdateFilmUI()
    {
        // Zde by mìla být implementace pro aktualizaci textu/ikon v herním rozhraní
        Debug.Log($"[UI] Zbývá snímkù: {currentFilmCount} / {maxFilmCount}");
    }
}