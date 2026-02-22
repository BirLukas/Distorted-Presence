using UnityEngine;
using System.Collections.Generic;

public class AnomalyManager : MonoBehaviour
{
    public static AnomalyManager Instance { get; private set; }
    public List<AnomalyController> anomalies = new List<AnomalyController>();

    public float minDelay = 10f;
    public float maxDelay = 20f;

    private float timer;
    private bool running = true;

    private void Awake() => Instance = this;

    void Start()
    {
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
            SanityManager.Instance.ApplyUnreportedPenalty(activeAnomalies.Count);
            Debug.Log($"Active anomalies: {activeAnomalies.Count}. Sanity is decreasing.");
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }

    void TriggerRandomAnomaly()
    {
        var inactive = anomalies.FindAll(a => !a.IsActive && !a.WasReported);

        if (inactive.Count == 0)
        {
            return;
        }

        int index = Random.Range(0, inactive.Count);
        inactive[index].Activate();

        Debug.Log("Activated anomaly: " + inactive[index].name);
    }

    /// <summary>
    /// Vrátí celkový počet anomálií, které byly aktivovány (aktivní nebo reportované).
    /// </summary>
    public int GetTotalTriggeredCount()
    {
        return anomalies.FindAll(a => a.IsActive || a.WasReported).Count;
    }

    /// <summary>
    /// Vrátí počet anomálií, které hráč úspěšně vyfotil.
    /// </summary>
    public int GetPhotographedCount()
    {
        return anomalies.FindAll(a => a.WasReported).Count;
    }

    /// <summary>
    /// Vrátí celkový počet anomálií v seznamu.
    /// </summary>
    public int GetTotalAnomalyCount()
    {
        return anomalies.Count;
    }

    /// <summary>
    /// Vrátí procento vyfocených anomálií z těch, které byly aktivovány.
    /// Pokud nebyla aktivována žádná, vrátí 100 (hráč nemusel nic fotit).
    /// </summary>
    public float GetPhotographedPercentage()
    {
        int triggered = GetTotalTriggeredCount();
        if (triggered == 0) return 100f;
        return (GetPhotographedCount() / (float)triggered) * 100f;
    }
}
