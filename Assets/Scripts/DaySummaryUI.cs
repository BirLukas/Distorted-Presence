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

    [Header("Colors")]
    public Color winColor = new Color(0.2f, 0.8f, 0.2f);
    public Color loseColor = new Color(0.8f, 0.2f, 0.2f);

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        isInitialized = true;
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
        if (summaryPanel == null) return;

        summaryPanel.SetActive(true);

        float percentage = totalCount > 0 ? (photographedCount / (float)totalCount) * 100f : 0f;

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
                resultDetailText.text = $"You need to photograph at least 70% of the anomalies.\nYou only photographed {percentage:F0}%.";
            }
        }
    }
}
