using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using TMPro;

public class SetupDaySummaryUI : EditorWindow
{
    [MenuItem("Tools/Setup DaySummary UI Buttons")]
    public static void Execute()
    {
        // Find DaySummaryPanel
        GameObject daySummaryPanel = GameObject.Find("DaySummaryPanel");
        if (daySummaryPanel == null)
        {
            // Try inactive objects
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                if (go.name == "DaySummaryPanel" && go.scene.IsValid())
                {
                    daySummaryPanel = go;
                    break;
                }
            }
        }

        if (daySummaryPanel == null)
        {
            Debug.LogError("DaySummaryPanel not found in scene!");
            return;
        }

        DaySummaryUI daySummaryUI = daySummaryPanel.GetComponent<DaySummaryUI>();
        if (daySummaryUI == null)
        {
            Debug.LogError("DaySummaryUI component not found on DaySummaryPanel!");
            return;
        }

        // Find existing PhotoGridParent
        Transform photoGridParent = daySummaryPanel.transform.Find("PhotoGridParent");
        if (photoGridParent == null)
        {
            Debug.LogError("PhotoGridParent not found under DaySummaryPanel!");
            return;
        }

        // ---- 1. Create PhotoReviewPanel ----
        GameObject photoReviewPanel = new GameObject("PhotoReviewPanel");
        photoReviewPanel.transform.SetParent(daySummaryPanel.transform, false);

        RectTransform photoReviewRT = photoReviewPanel.AddComponent<RectTransform>();
        // Stretch full width, anchored at bottom
        photoReviewRT.anchorMin = new Vector2(0f, 0f);
        photoReviewRT.anchorMax = new Vector2(1f, 0f);
        photoReviewRT.pivot = new Vector2(0.5f, 0f);
        photoReviewRT.anchoredPosition = new Vector2(0f, 20f);
        photoReviewRT.sizeDelta = new Vector2(-40f, 220f);

        // Add background image
        Image bgImage = photoReviewPanel.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.04f, 0.03f, 0.95f);

        // ---- 2. Move PhotoGridParent into PhotoReviewPanel ----
        photoGridParent.SetParent(photoReviewPanel.transform, false);
        // Reset its position inside the new parent
        RectTransform gridRT = photoGridParent.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0f, 0f);
        gridRT.anchorMax = new Vector2(1f, 1f);
        gridRT.anchoredPosition = new Vector2(0f, 0f);
        gridRT.sizeDelta = new Vector2(-20f, -10f);
        gridRT.pivot = new Vector2(0.5f, 0.5f);

        // ---- 3. Set photoReviewPanel reference on DaySummaryUI ----
        Undo.RecordObject(daySummaryUI, "Setup DaySummary UI");
        daySummaryUI.photoReviewPanel = photoReviewPanel;
        EditorUtility.SetDirty(daySummaryUI);

        // ---- 4. Create "View Photos" Button ----
        // Find Card to position buttons below it
        Transform card = daySummaryPanel.transform.Find("Card");
        float cardBottom = 0f;
        if (card != null)
        {
            RectTransform cardRT = card.GetComponent<RectTransform>();
            // Card is centered, so bottom = anchoredPosition.y - sizeDelta.y/2
            cardBottom = cardRT.anchoredPosition.y - cardRT.sizeDelta.y / 2f;
        }

        // Button container (horizontal layout)
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(daySummaryPanel.transform, false);
        RectTransform containerRT = buttonContainer.AddComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 1f);
        // Position just below the card
        containerRT.anchoredPosition = new Vector2(0f, cardBottom - 10f);
        containerRT.sizeDelta = new Vector2(500f, 50f);

        HorizontalLayoutGroup hlg = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // --- View Photos Button ---
        GameObject viewPhotosBtn = CreateButton("ViewPhotosButton", "View Photos", buttonContainer.transform);
        RectTransform vpRT = viewPhotosBtn.GetComponent<RectTransform>();
        vpRT.sizeDelta = new Vector2(220f, 45f);

        Button vpButton = viewPhotosBtn.GetComponent<Button>();
        ColorBlock vpColors = vpButton.colors;
        vpColors.normalColor = new Color(0.2f, 0.4f, 0.6f, 1f);
        vpColors.highlightedColor = new Color(0.3f, 0.55f, 0.8f, 1f);
        vpColors.pressedColor = new Color(0.15f, 0.3f, 0.5f, 1f);
        vpButton.colors = vpColors;

        // Add OnClick -> DaySummaryUI.TogglePhotoReview
        UnityAction toggleAction = System.Delegate.CreateDelegate(typeof(UnityAction), daySummaryUI, "TogglePhotoReview") as UnityAction;
        UnityEventTools.AddPersistentListener(vpButton.onClick, toggleAction);
        EditorUtility.SetDirty(vpButton);

        // --- Next Day / Restart Button ---
        GameObject nextDayBtn = CreateButton("NextDayButton", "Next Day / Restart", buttonContainer.transform);
        RectTransform ndRT = nextDayBtn.GetComponent<RectTransform>();
        ndRT.sizeDelta = new Vector2(220f, 45f);

        Button ndButton = nextDayBtn.GetComponent<Button>();
        ColorBlock ndColors = ndButton.colors;
        ndColors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        ndColors.highlightedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        ndColors.pressedColor = new Color(0.15f, 0.45f, 0.15f, 1f);
        ndButton.colors = ndColors;

        // Add OnClick -> DaySummaryUI.OnNextDayClicked
        UnityAction nextDayAction = System.Delegate.CreateDelegate(typeof(UnityAction), daySummaryUI, "OnNextDayClicked") as UnityAction;
        UnityEventTools.AddPersistentListener(ndButton.onClick, nextDayAction);
        EditorUtility.SetDirty(ndButton);

        // ---- Register Undo for new objects ----
        Undo.RegisterCreatedObjectUndo(photoReviewPanel, "Create PhotoReviewPanel");
        Undo.RegisterCreatedObjectUndo(buttonContainer, "Create ButtonContainer");

        // ---- Save scene ----
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(daySummaryPanel.scene);

        Debug.Log("[SetupDaySummaryUI] Done! PhotoReviewPanel created, PhotoGridParent moved, buttons created with OnClick events.");
    }

    private static GameObject CreateButton(string name, string label, Transform parent)
    {
        // Root button object
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 45f);

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;

        // Text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);

        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        textRT.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnGO;
    }
}
