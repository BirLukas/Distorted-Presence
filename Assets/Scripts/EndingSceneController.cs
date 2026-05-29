using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EndingSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI rankText;
    public RawImage photoDisplay;
    public float photoChangeInterval = 2.5f;

    void Start()
    {
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameProgressionManager.Instance != null)
        {
            int totalFound = GameProgressionManager.Instance.globalPhotographed;
            int totalTriggered = GameProgressionManager.Instance.globalTriggered;
            float percentage = totalTriggered > 0 ? (totalFound / (float)totalTriggered) * 100f : 100f;

            string rank = GetRank(percentage);

            if (statsText != null)
            {
                statsText.text = $"Total\nAnomalies Documented:\n<b>{totalFound} / {totalTriggered}</b>\n\nAccuracy:\n<b>{percentage:F0}%</b>";
            }

            if (rankText != null)
            {
                rankText.text = $"FINAL RANK:\n{rank}";
                rankText.color = GetRankColor(rank);
            }

            if (photoDisplay != null && GameProgressionManager.Instance.globalPhotos.Count > 0)
            {
                StartCoroutine(CyclePhotos(GameProgressionManager.Instance.globalPhotos));
            }
            else if (photoDisplay != null)
            {
                photoDisplay.gameObject.SetActive(false);
            }
        }
    }

    private string GetRank(float percentage)
    {
        if (percentage >= 100f) return "S";
        if (percentage >= 90f) return "A";
        if (percentage >= 75f) return "B";
        if (percentage >= 60f) return "C";
        return "D";
    }

    private Color GetRankColor(string rank)
    {
        switch (rank)
        {
            case "S": return Color.cyan;
            case "A": return Color.green;
            case "B": return Color.yellow;
            case "C": return new Color(1f, 0.5f, 0f); // Orange
            default: return Color.red;
        }
    }

    private IEnumerator CyclePhotos(List<Texture2D> photos)
    {
        int index = 0;
        while (true)
        {
            photoDisplay.texture = photos[index];
            index = (index + 1) % photos.Count;
            yield return new WaitForSeconds(photoChangeInterval);
        }
    }

    public void OnMainMenuClicked()
    {
        if (GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.ResetProgression();
        }
        Time.timeScale = 1f; // Just in case
        SceneManager.LoadScene("MainMenu");
    }
}
