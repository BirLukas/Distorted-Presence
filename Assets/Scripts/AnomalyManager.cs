using UnityEngine;
using System.Collections.Generic;

public class AnomalyManager : MonoBehaviour
{
    public List<AnomalyController> anomalies = new List<AnomalyController>();

    public float minDelay = 10f;
    public float maxDelay = 20f;

    private float timer;
    private bool running = true;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!running) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            TriggerRandomAnomaly();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }

    void TriggerRandomAnomaly()
    {
        var inactive = anomalies.FindAll(a => !a.IsActive);

        if (inactive.Count == 0)
        {
            Debug.Log("All anomalies are active.");
            running = false;
            return;
        }

        int index = Random.Range(0, inactive.Count);
        inactive[index].Activate();

        Debug.Log("Activated anomaly: " + inactive[index].name);
    }
}