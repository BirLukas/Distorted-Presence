using UnityEngine;
using UnityEngine.Events;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    [Header("Settings")]
    [Range(0f, 100f)]
    private float currentSanity = 100f;
    public float CurrentSanity => currentSanity;

    public float correctAnomalyBonus = 5f;
    public float incorrectAnomalyPenalty = 10f;
    public float unreportedAnomalyPenalty = 1f;

    [Header("Events & UI")]
    public UnityEvent OnSanityZero;
    public UnityEvent OnVictory;
    public UnityEvent OnDefeat;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddSanity(float amount)
    {
        if (isGameOver) return;
        currentSanity = Mathf.Clamp(currentSanity + amount, 0f, 100f);
        CheckForLossCondition();
    }

    public void RemoveSanity(float amount)
    {
        if (isGameOver) return;
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, 100f);
        CheckForLossCondition();
    }

    public void ApplyUnreportedPenalty(int unreportedCount)
    {
        if (isGameOver) return;
        float penalty = unreportedCount * unreportedAnomalyPenalty * Time.deltaTime;
        RemoveSanity(penalty);
    }

    private void CheckForLossCondition()
    {
        if (isGameOver) return;
        if (currentSanity <= 0.001f)
        {
            currentSanity = 0f;
            isGameOver = true;
            StopGame();

            DaySummaryUI ui = DaySummaryUI.Instance;
            if (ui == null)
            {
                ui = FindFirstObjectByType<DaySummaryUI>(FindObjectsInactive.Include);
            }

            if (ui != null)
            {
                int photographed = AnomalyManager.Instance != null ? AnomalyManager.Instance.GetPhotographedCount() : 0;
                int triggered = AnomalyManager.Instance != null ? AnomalyManager.Instance.GetTotalTriggeredCount() : 0;
                ui.ShowSummary(false, 0f, photographed, triggered, "You've gone insane because of insanity!");
            }
            else
            {
                Debug.LogError("[SanityManager] DaySummaryUI not found in scene!");
            }

            OnSanityZero.Invoke();
        }
    }

    public void Victory()
    {
        if (isGameOver) return;
        isGameOver = true;
        StopGame();
        OnVictory.Invoke();
    }

    public void Defeat()
    {
        if (isGameOver) return;
        isGameOver = true;
        StopGame();
        OnDefeat.Invoke();
    }

    private void StopGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResetSanity(float initialValue = 100f)
    {
        currentSanity = initialValue;
        isGameOver = false;
        Time.timeScale = 1f;
    }
}
