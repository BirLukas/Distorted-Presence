using UnityEngine;
using System.Collections.Generic;

public class AnomalyManager : MonoBehaviour
{
    public static AnomalyManager Instance { get; private set; }
    
    [Header("Anomalies by Day")]
    public List<AnomalyController> day1Anomalies = new List<AnomalyController>();
    public List<AnomalyController> day2Anomalies = new List<AnomalyController>();
    [Tooltip("Anomalies for days 3, 4, and 5")]
    public List<AnomalyController> day3To5Anomalies = new List<AnomalyController>();

    [HideInInspector]
    public List<AnomalyController> anomalies = new List<AnomalyController>();

    public float minDelay = 10f;
    public float maxDelay = 20f;

    private float timer;
    private bool running = true;

    private void Awake() => Instance = this;

    // Počet doposud vygenerovaných anomálií
    private int generatedAnomaliesToday = 0;

    void Start()
    {
        if (GameProgressionManager.Instance != null)
        {
            minDelay = GameProgressionManager.Instance.CurrentMinDelay;
            maxDelay = GameProgressionManager.Instance.CurrentMaxDelay;
            
            int currentDay = GameProgressionManager.Instance.currentDay;

            if (currentDay == 1)
            {
                anomalies = new List<AnomalyController>(day1Anomalies);
                GameProgressionManager.Instance.activatedAnomalyNames.Clear();
            }
            else if (currentDay == 2)
            {
                anomalies = new List<AnomalyController>(day2Anomalies);
            }
            else
            {
                anomalies = new List<AnomalyController>(day3To5Anomalies);
            }
        }
        else
        {
            anomalies = new List<AnomalyController>(day1Anomalies);
        }

        ResetTimer();
    }

    void Update()
    {
        if (SanityManager.Instance != null && SanityManager.Instance.IsGameOver)
        {
            running = false;
            return;
        }

        if (!running) return;

        // Anomálie se nespouští, dokud nezačne běžet herní čas (Den 1 všechny místnosti, další dny zpoždění)
        if (DayManager.Instance != null && !DayManager.Instance.HasStarted)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            TriggerRandomAnomaly();
            ResetTimer();
        }

        CheckUnreportedAnomalies();
    }

    void CheckUnreportedAnomalies()
    {
        if (SanityManager.Instance == null) return;

        var activeAnomalies = anomalies.FindAll(a => a.IsActive);

        if (activeAnomalies.Count > 0)
        {
            // Aplikujeme penalizaci podle aktuálního dne
            float multiplier = GameProgressionManager.Instance != null ? GameProgressionManager.Instance.CurrentSanityPenalty : 1f;
            SanityManager.Instance.ApplyUnreportedPenalty(activeAnomalies.Count * (int)multiplier);
            // Debug.Log($"Active anomalies: {activeAnomalies.Count}. Sanity is decreasing.");
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }

    void TriggerRandomAnomaly()
    {
        if (GameProgressionManager.Instance != null && generatedAnomaliesToday >= GameProgressionManager.Instance.MaxAnomaliesPerDay)
        {
            // Maximální počet dosažen, další se nebudou spawnovat.
            return;
        }

        var inactive = anomalies.FindAll(a => 
            !a.IsActive && 
            !a.WasReported && 
            (GameProgressionManager.Instance == null || !GameProgressionManager.Instance.activatedAnomalyNames.Contains(a.gameObject.name))
        );

        if (inactive.Count == 0)
        {
            return;
        }

        int index = Random.Range(0, inactive.Count);
        var selectedAnomaly = inactive[index];
        selectedAnomaly.Activate();
        
        if (GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.activatedAnomalyNames.Add(selectedAnomaly.gameObject.name);
        }
        
        generatedAnomaliesToday++;

        Debug.Log($"Activated anomaly: {selectedAnomaly.name} ({generatedAnomaliesToday}/{GameProgressionManager.Instance?.MaxAnomaliesPerDay ?? 0})");
    }

    public int GetTotalTriggeredCount()
    {
        return anomalies.FindAll(a => a.IsActive || a.WasReported).Count;
    }

    public int GetPhotographedCount()
    {
        return anomalies.FindAll(a => a.WasReported).Count;
    }

    public int GetTotalAnomalyCount()
    {
        return anomalies.Count;
    }

    public float GetPhotographedPercentage()
    {
        int triggered = GetTotalTriggeredCount();
        if (triggered == 0) return 100f;
        return (GetPhotographedCount() / (float)triggered) * 100f;
    }
}
