using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("Day Settings")]
    public float dayDuration = 120f;
    private float timer;
    private bool isDayEnded = false;

    [Header("Win Condition")]
    [Range(0f, 100f)]
    public float requiredAnomalyPercentage = 70f;

    [Header("UI References")]
    public TextMeshProUGUI clockText;

    void Start()
    {
        timer = dayDuration;
    }

    void Update()
    {
        bool isGameOver = SanityManager.Instance != null && SanityManager.Instance.IsGameOver;

        if (isGameOver || isDayEnded)
        {
            if (clockText != null && clockText.gameObject.activeSelf)
                clockText.gameObject.SetActive(false);
            return;
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateClockUI();
        }
        else
        {
            EndDay();
        }
    }

    void UpdateClockUI()
    {
        if (clockText == null) return;

        // Progress from 00:00 to 06:00
        float progress = 1 - (timer / dayDuration);
        int hours = Mathf.FloorToInt(progress * 6);
        int minutes = Mathf.FloorToInt((progress * 360) % 60);
        clockText.text = string.Format("{0:00}:{1:00} AM", hours, minutes);
    }

    /// <summary>
    /// Ukončí den – zkontroluje podmínky výhry/prohry a zobrazí souhrn.
    /// Volá se buď uplynutím času, nebo odchodem dveřmi.
    /// </summary>
    public void EndDay()
    {
        if (isDayEnded) return;
        isDayEnded = true;

        if (clockText != null && clockText.gameObject.activeSelf)
            clockText.gameObject.SetActive(false);

        // Zastav hru
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Zjisti výsledky anomálií
        int photographed = 0;
        int triggered = 0;

        if (AnomalyManager.Instance != null)
        {
            photographed = AnomalyManager.Instance.GetPhotographedCount();
            
            // Total by se měl vázat spíš na celkový limit obsažený v Progression
            if (GameProgressionManager.Instance != null)
                triggered = GameProgressionManager.Instance.MaxAnomaliesPerDay;
            else
                triggered = AnomalyManager.Instance.GetTotalTriggeredCount();
        }

        float percentage = triggered > 0 ? (photographed / (float)triggered) * 100f : 100f;
        bool isVictory = percentage >= requiredAnomalyPercentage;

        float sanity = SanityManager.Instance != null ? SanityManager.Instance.CurrentSanity : 0f;

        // Zobraz souhrn dne
        DaySummaryUI ui = DaySummaryUI.Instance;
        if (ui == null)
        {
            ui = FindFirstObjectByType<DaySummaryUI>(FindObjectsInactive.Include);
        }

        if (ui != null)
        {
            ui.ShowSummary(isVictory, sanity, photographed, triggered);
        }
        else
        {
            Debug.LogError("[DayManager] DaySummaryUI not found in scene!");
        }

        // Posun dne pokud je výhra
        if (isVictory && GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.AdvanceDay();
        }

        // Informuj SanityManager o výsledku (pro případné eventy)
        if (SanityManager.Instance != null)
        {
            if (isVictory)
                SanityManager.Instance.Victory();
            else
                SanityManager.Instance.Defeat();
        }
    }
}
