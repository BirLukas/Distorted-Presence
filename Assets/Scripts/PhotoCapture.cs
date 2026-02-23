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
    public AudioSource AudioSource;
    public AudioClip ShutterSound;
    
    [Header("Photo History")]
    public System.Collections.Generic.List<PhotoData> takenPhotos = new System.Collections.Generic.List<PhotoData>();


    void Start()
    {
        currentFilmCount = maxFilmCount;
        UpdateFilmUI();
        takenPhotos.Clear();

        if (noFilmWarningUI != null)
        {
            noFilmWarningUI.SetActive(false);
        }
    }
    public void TryCaptureAnomaly(System.Action onSnapshotCaptured = null)
    {
        if (currentFilmCount <= 0)
        {
            ShowNoFilmWarning();
            return;
        }

        currentFilmCount--;
        UpdateFilmUI();

        if (AudioSource != null && ShutterSound != null)
        {
            AudioSource.PlayOneShot(ShutterSound);
        }

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDistance, anomalyLayer))
        {
            AnomalyController anomaly = hit.collider.GetComponentInParent<AnomalyController>();

            if (anomaly != null && anomaly.IsActive)
            {
                ProcessAnomalyCapture(anomaly);
                StartCoroutine(CaptureScreenshotRoutine(true, anomaly.gameObject.name, onSnapshotCaptured));
            }
            else
            {
                ApplyIncorrectCapturePenalty();
                StartCoroutine(CaptureScreenshotRoutine(false, hit.collider.gameObject.name, onSnapshotCaptured));
            }
        }
        else
        {
            ApplyIncorrectCapturePenalty();
            StartCoroutine(CaptureScreenshotRoutine(false, "Unknown", onSnapshotCaptured));
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

            Debug.Log($"Anomaly {anomaly.gameObject.name} reported. Sanity + {SanityManager.Instance.correctAnomalyBonus}%");
        }
        else
        {
            ApplyIncorrectCapturePenalty();

            Debug.LogWarning($"Wrong picture! Object {anomaly.gameObject.name} is not an anomaly.");
        }
    }

    private void ApplyIncorrectCapturePenalty()
    {
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.RemoveSanity(SanityManager.Instance.incorrectAnomalyPenalty);
            Debug.Log($"Wrong Picture. Sanity - {SanityManager.Instance.incorrectAnomalyPenalty}%");
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

    private System.Collections.IEnumerator CaptureScreenshotRoutine(bool isCorrect, string targetName, System.Action onSnapshotCaptured)
    {
        // Čekáme pouze na konec framu (než se vyrenderuje UI bez blesku)
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        PhotoData pd = new PhotoData();
        pd.snapshot = tex;
        pd.isCorrect = isCorrect;
        pd.targetName = targetName;

        takenPhotos.Add(pd);

        // Nyní odpálíme blesk!
        onSnapshotCaptured?.Invoke();
    }
}
