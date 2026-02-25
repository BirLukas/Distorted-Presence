using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DaySummaryUI : MonoBehaviour
{
    public static DaySummaryUI Instance { get; private set; }

    [Header("Panel References")]
    public GameObject summaryPanel;

    [Header("Text References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI sanityText;
    public TextMeshProUGUI anomalyText;
    public TextMeshProUGUI resultDetailText;
    public TextMeshProUGUI daysLeftText; // Nový text pro počet dnů

    [Header("Photo Grid")]
    public GameObject photoReviewPanel; // Panel, který obsahuje Grid Parent (lze ho skrývat/zobrazovat)
    public Transform photoGridParent; // Rodič pro layout (Grid Layout Group)
    public GameObject photoPrefab; // Prefab pro PhotoReviewUI

    [Header("Colors")]
    public Color winColor = new Color(0.2f, 0.8f, 0.2f);
    public Color loseColor = new Color(0.8f, 0.2f, 0.2f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (summaryPanel != null && summaryPanel.activeSelf && Time.timeSinceLevelLoad < 1f)
        {
            summaryPanel.SetActive(false);
        }
    }

    public void ShowSummary(bool isVictory, float sanity, int photographedCount, int totalCount, string reason = "")
    {
        lastResultWasVictory = isVictory;
        if (summaryPanel == null) return;

        summaryPanel.SetActive(true);

        float percentage = totalCount > 0 ? (photographedCount / (float)totalCount) * 100f : 0f;

        // Dny do konce
        if (daysLeftText != null && GameProgressionManager.Instance != null)
        {
            daysLeftText.text = $"Day <b>{GameProgressionManager.Instance.currentDay}</b> / {GameProgressionManager.Instance.maxDays}";
        }

        // Titulek
        if (titleText != null)
        {
            titleText.text = isVictory ? "You survived the day" : "You didn't survive the day";
            titleText.color = isVictory ? winColor : loseColor;
        }

        // Sanita
        if (sanityText != null)
        {
            sanityText.text = $"Remaining sanity: <b>{sanity:F0}%</b>";
            sanityText.color = sanity > 50f ? winColor : (sanity > 20f ? Color.yellow : loseColor);
        }

        // Anomálie
        if (anomalyText != null)
        {
            anomalyText.text = $"Photographed anomalies: <b>{photographedCount} / {totalCount}</b>  ({percentage:F0}%)";
            anomalyText.color = isVictory ? winColor : loseColor;
        }

        // Detail výsledku
        if (resultDetailText != null)
        {
            if (!string.IsNullOrEmpty(reason))
            {
                resultDetailText.text = reason;
            }
            else if (isVictory)
            {
                resultDetailText.text = "You documented enough anomalies.";
            }
            else
            {
                resultDetailText.text = $"You need to photograph at least {(percentage > 0 ? "more" : "some")} anomalies.\nYou only photographed {percentage:F0}%.";
            }
        }

        // Zobrazení historie vyfocených snímků posbíráme na pozadí
        PopulatePhotoGrid();

        // Panel s fotkami schováme ze začátku, dokud ho hráč nezapne
        if (photoReviewPanel != null)
        {
            photoReviewPanel.SetActive(false);
        }
    }

    private void PopulatePhotoGrid()
    {
        if (photoGridParent == null || photoPrefab == null) return;

        // Smažeme případné staré fotky
        foreach (Transform child in photoGridParent)
        {
            Destroy(child.gameObject);
        }

        PhotoCapture pCapture = FindFirstObjectByType<PhotoCapture>();
        if (pCapture == null) return;

        foreach (var pData in pCapture.takenPhotos)
        {
            GameObject obj = Instantiate(photoPrefab, photoGridParent);
            PhotoReviewUI pUI = obj.GetComponent<PhotoReviewUI>();
            
            if (pUI != null)
            {
                pUI.Setup(pData);
            }
        }
    }

    // Volat přes OnClick v Unity u tlačítka "Zobrazit / Skrýt Fotky"
    public void TogglePhotoReview()
    {
        if (photoReviewPanel != null)
        {
            photoReviewPanel.SetActive(!photoReviewPanel.activeSelf);
        }
    }

    private bool lastResultWasVictory;

    // Volat přes OnClick v Unity u tlačítka "Další den" nebo "Restart"
    public void OnNextDayClicked()
    {
        // Vrátíme čas do normálu
        Time.timeScale = 1f;

        if (GameProgressionManager.Instance != null)
        {
            if (lastResultWasVictory)
            {
                // Jdeme na další den - reload aktuální scény pro restart anomálií a časovače
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                // Smrt (Nebo dohráno) -> Chtělo by to restart scén 
                GameProgressionManager.Instance.ResetProgression();
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            // Fallback (např. pokud se testuje bez Manageru)
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
