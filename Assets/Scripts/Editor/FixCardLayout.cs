using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class FixCardLayout
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
        if (card == null) { Debug.LogError("Card not found!"); return; }

        // ---- Make Card stretch to fill the panel with padding ----
        // This way it always fits the screen regardless of resolution
        RectTransform cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0f, 0f);
        cardRT.anchorMax = new Vector2(1f, 1f);
        cardRT.pivot = new Vector2(0.5f, 0.5f);
        cardRT.anchoredPosition = new Vector2(0f, 0f);
        // Padding: 20px left/right, 20px top, 10px bottom
        cardRT.offsetMin = new Vector2(20f, 10f);   // left, bottom
        cardRT.offsetMax = new Vector2(-20f, -20f); // -right, -top

        // ---- Fix ButtonContainer position inside Card ----
        Transform buttonContainer = card.Find("ButtonContainer");
        if (buttonContainer != null)
        {
            RectTransform btnRT = buttonContainer.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, 0f);
            btnRT.anchorMax = new Vector2(1f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0f, 12f);
            btnRT.sizeDelta = new Vector2(-40f, 52f);

            HorizontalLayoutGroup hlg = buttonContainer.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.spacing = 20f;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
                hlg.childControlWidth = false;
                hlg.childControlHeight = true;
                hlg.padding = new RectOffset(10, 10, 4, 4);
            }

            // Fix button sizes
            Transform vpBtn = buttonContainer.Find("ViewPhotosButton");
            Transform ndBtn = buttonContainer.Find("NextDayButton");
            if (vpBtn != null)
            {
                RectTransform rt = vpBtn.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(220f, 44f);
                Image img = vpBtn.GetComponent<Image>();
                if (img != null) img.color = new Color(0.15f, 0.35f, 0.65f, 1f);
                TextMeshProUGUI tmp = vpBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) { tmp.color = Color.white; tmp.fontSize = 16f; tmp.fontStyle = FontStyles.Bold; }
            }
            if (ndBtn != null)
            {
                RectTransform rt = ndBtn.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(220f, 44f);
                Image img = ndBtn.GetComponent<Image>();
                if (img != null) img.color = new Color(0.1f, 0.48f, 0.15f, 1f);
                TextMeshProUGUI tmp = ndBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) { tmp.color = Color.white; tmp.fontSize = 16f; tmp.fontStyle = FontStyles.Bold; }
            }
        }

        EditorUtility.SetDirty(daySummaryPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(daySummaryPanel.scene);
        Debug.Log("[FixCardLayout] Done! Card uses stretch anchors, buttons at bottom.");
    }
}
