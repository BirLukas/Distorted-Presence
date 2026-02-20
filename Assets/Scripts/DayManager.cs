using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("Day Settings")]
    public float dayDuration = 120f;
    private float timer;
    private bool isDayEnded = false;

    [Header("UI References")]
    public TextMeshProUGUI clockText;

    void Start()
    {
        timer = dayDuration;
    }

    void Update()
    {
        if (isDayEnded) return;

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

    public void EndDay()
    {
        isDayEnded = true;
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.Victory();
        }
    }
}
