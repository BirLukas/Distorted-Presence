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
    public void TryCaptureAnomaly()
    {
        if (currentFilmCount <= 0)
        {
            Debug.Log("Došly snímky! Už nemùžeš fotit.");
            return;
        }

        currentFilmCount--;
        UpdateFilmUI();

        if (cameraShutterSound != null)
        {
            cameraShutterSound.Play();
        }

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDistance, anomalyLayer))
        {
            AnomalyController anomaly = hit.collider.GetComponentInParent<AnomalyController>();

            if (anomaly != null)
            {
                ProcessAnomalyCapture(anomaly);
            }
            else
            {
                ApplyIncorrectCapturePenalty();
            }
        }
        else
        {
            ApplyIncorrectCapturePenalty();
        }
    }

    private void ProcessAnomalyCapture(AnomalyController anomaly)
    {
        if (SanityManager.Instance == null) return;

        if (anomaly.IsActive)
        {
            SanityManager.Instance.AddSanity(SanityManager.Instance.correctAnomalyBonus);

            anomaly.ResetAnomaly();

            Debug.Log($"Anomálie {anomaly.gameObject.name} nahlášena. Sanity + {SanityManager.Instance.correctAnomalyBonus}%");
        }
        else
        {
            ApplyIncorrectCapturePenalty();

            Debug.LogWarning($"Špatná fotka! Objekt {anomaly.gameObject.name} není anomálie.");
        }
    }

    private void ApplyIncorrectCapturePenalty()
    {
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.RemoveSanity(SanityManager.Instance.incorrectAnomalyPenalty);
            Debug.Log($"Špatná fotka. Sanity - {SanityManager.Instance.incorrectAnomalyPenalty}%");
        }
    }

    public void RefillFilm()
    {
        currentFilmCount = maxFilmCount;
        UpdateFilmUI();
    }

    public int GetRemainingFilmCount()
    {
        return currentFilmCount;
    }

    private void UpdateFilmUI()
    {
        Debug.Log($"Zbývá snímkù: {currentFilmCount} / {maxFilmCount}");
    }
}