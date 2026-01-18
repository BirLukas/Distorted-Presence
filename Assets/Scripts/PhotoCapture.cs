using UnityEngine;
using TMPro;
public class PhotoCapture : MonoBehaviour
{
    [Header("Capture Settings")]
    public float raycastDistance = 15f;
    public LayerMask anomalyLayer;

    [Header("Ammo Settings")]
    public int maxFilmCount = 10;
    private int currentFilmCount;

    [Header("UI Settings")]
    public TextMeshProUGUI filmCountText;
    public GameObject noFilmWarningUI;

    [Header("Camera Effect Settings")]
    public AudioSource cameraShutterSound;

    void Start()
    {
        currentFilmCount = maxFilmCount;
        UpdateFilmUI();

        if (noFilmWarningUI != null)
        {
            noFilmWarningUI.SetActive(false);
        }
    }
    public void TryCaptureAnomaly()
    {
        if (currentFilmCount <= 0)
        {
            ShowNoFilmWarning();
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

            if (anomaly != null && anomaly.IsActive)
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
    private void ShowNoFilmWarning()
    {
        if (noFilmWarningUI != null)
        {
            noFilmWarningUI.SetActive(true);
            CancelInvoke("HideNoFilmWarning");
            Invoke("HideNoFilmWarning", 2f);
        }
    }

    private void HideNoFilmWarning()
    {
        if (noFilmWarningUI != null) noFilmWarningUI.SetActive(false);
    }

    private void ProcessAnomalyCapture(AnomalyController anomaly)
    {
        if (SanityManager.Instance == null) return;

        if (anomaly.IsActive)
        {
            SanityManager.Instance.AddSanity(SanityManager.Instance.correctAnomalyBonus);

            anomaly.ReportAnomaly();

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
        filmCountText.text = currentFilmCount.ToString();

        if (currentFilmCount <= 0)
        {
            filmCountText.color = Color.red;
        }
        else
        {
            filmCountText.color = Color.green;
        }
    }
}