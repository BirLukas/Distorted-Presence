using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class FixButtonLayout
{
    public static void Execute()
    {
        // Find DaySummaryPanel (including inactive)
        GameObject daySummaryPanel = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "DaySummaryPanel" && go.scene.IsValid())
            {
                daySummaryPanel = go;
                break;
            }
        }
        if (daySummaryPanel == null) { Debug.LogError("DaySummaryPanel not found!"); return; }

        Transform card = daySummaryPanel.transform.Find("Card");
        Transform buttonContainer = daySummaryPanel.transform.Find("ButtonContainer");
        Transform photoReviewPanel = daySummaryPanel.transform.Find("PhotoReviewPanel");

        if (card == null) { Debug.LogError("Card not found!"); return; }
        if (buttonContainer == null) { Debug.LogError("ButtonContainer not found!"); return; }

        // ---- Move ButtonContainer INTO Card ----
        Undo.SetTransformParent(buttonContainer, card, "Move ButtonContainer into Card");

        // Position at bottom of Card, centered
        RectTransform btnRT = buttonContainer.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0f, 0f);
        btnRT.anchorMax = new Vector2(1f, 0f);
        btnRT.pivot = new Vector2(0.5f, 0f);
        btnRT.anchoredPosition = new Vector2(0f, 15f);
        btnRT.sizeDelta = new Vector2(0f, 55f);

        // Make HorizontalLayoutGroup fill properly
        HorizontalLayoutGroup hlg = buttonContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 20f;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(20, 20, 5, 5);
        }

        // Fix button sizes
        Transform vpBtn = buttonContainer.Find("ViewPhotosButton");
        Transform ndBtn = buttonContainer.Find("NextDayButton");

        if (vpBtn != null)
        {
            RectTransform rt = vpBtn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220f, 45f);
        }
        if (ndBtn != null)
        {
            RectTransform rt = ndBtn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220f, 45f);
        }

        // ---- Resize Card to accommodate buttons ----
        // Original card height was 472.32, add ~70px for buttons at bottom
        RectTransform cardRT = card.GetComponent<RectTransform>();
        float newHeight = 472.32f + 70f;
        cardRT.sizeDelta = new Vector2(cardRT.sizeDelta.x, newHeight);
        // Shift card up slightly so it stays centered but buttons are visible
        cardRT.anchoredPosition = new Vector2(cardRT.anchoredPosition.x, 35f);

        // ---- Reposition PhotoReviewPanel below the card ----
        if (photoReviewPanel != null)
        {
            RectTransform prRT = photoReviewPanel.GetComponent<RectTransform>();
            // Anchor to bottom of screen, full width
            prRT.anchorMin = new Vector2(0f, 0f);
            prRT.anchorMax = new Vector2(1f, 0f);
            prRT.pivot = new Vector2(0.5f, 0f);
            prRT.anchoredPosition = new Vector2(0f, 10f);
            prRT.sizeDelta = new Vector2(-40f, 220f);
        }

        EditorUtility.SetDirty(daySummaryPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(daySummaryPanel.scene);
        Debug.Log("[FixButtonLayout] Done! Buttons moved inside Card.");
    }
}
