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

    [Header("Button References")]
    public TextMeshProUGUI actionButtonText; // Reference na text tlačítka "Další den / Restart"

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

        // Dynamické vyhledání textu tlačítka, pokud není přiřazeno v Inspektoru
        if (actionButtonText == null)
        {
            foreach (var btn in summaryPanel.GetComponentsInChildren<Button>(true))
            {
                if (btn.gameObject.name == "NextDayButton" || btn.gameObject.name.Contains("NextDay") || btn.gameObject.name.Contains("Next"))
                {
                    actionButtonText = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (actionButtonText != null)
                        break;
                }
            }
        }

        bool isFinalDay = (GameProgressionManager.Instance != null && GameProgressionManager.Instance.currentDay == GameProgressionManager.Instance.maxDays);

        // Nastavení textu tlačítka podle výsledku
        if (actionButtonText != null)
        {
            if (isVictory)
            {
                actionButtonText.text = isFinalDay ? "Finish Game" : "Next Day";
            }
            else
            {
                actionButtonText.text = "Restart";
            }
        }

        float percentage = totalCount > 0 ? (photographedCount / (float)totalCount) * 100f : 0f;

        // Dny do konce
        if (daysLeftText != null && GameProgressionManager.Instance != null)
        {
            daysLeftText.text = $"Day <b>{GameProgressionManager.Instance.currentDay}</b> / {GameProgressionManager.Instance.maxDays}";
        }

        // Titulek
        if (titleText != null)
        {
            if (isVictory)
            {
                titleText.text = isFinalDay ? "You survived all 5 days!" : "You survived the day";
                titleText.color = winColor;
            }
            else
            {
                titleText.text = "You didn't survive the day";
                titleText.color = loseColor;
            }
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
            if (GameProgressionManager.Instance.IsGameFinished)
            {
                // Hra dohrána -> Načteme závěrečnou scénu se statistikami
                UnityEngine.SceneManagement.SceneManager.LoadScene("EndingScene");
            }
            else if (lastResultWasVictory)
            {
                // Jdeme na další den - reload aktuální scény pro restart anomálií a časovače
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                // Smrt -> restart scény a progrese
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
