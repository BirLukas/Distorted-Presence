using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using TMPro;

public class FixPhotoReviewPanel
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

        DaySummaryUI daySummaryUI = daySummaryPanel.GetComponent<DaySummaryUI>();
        if (daySummaryUI == null) { Debug.LogError("DaySummaryUI not found!"); return; }

        Transform photoReviewPanel = daySummaryPanel.transform.Find("PhotoReviewPanel");
        if (photoReviewPanel == null) { Debug.LogError("PhotoReviewPanel not found!"); return; }

        // ---- 1. Fix Image raycastTarget so it doesn't block clicks ----
        Image panelImage = photoReviewPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.raycastTarget = false;
        }

        // ---- 2. Fix PhotoGridParent raycastTarget ----
        Transform photoGridParent = photoReviewPanel.Find("PhotoGridParent");
        // GridLayoutGroup doesn't have Image, but check children just in case

        // ---- 3. Add a header bar with title + close button ----
        // Check if header already exists
        Transform existingHeader = photoReviewPanel.Find("Header");
        if (existingHeader != null)
        {
            Object.DestroyImmediate(existingHeader.gameObject);
        }

        GameObject header = new GameObject("Header");
        header.transform.SetParent(photoReviewPanel, false);
        header.transform.SetAsFirstSibling();

        RectTransform headerRT = header.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0f, 1f);
        headerRT.anchorMax = new Vector2(1f, 1f);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = new Vector2(0f, 0f);
        headerRT.sizeDelta = new Vector2(0f, 36f);

        Image headerImg = header.AddComponent<Image>();
        headerImg.color = new Color(0.08f, 0.06f, 0.04f, 0.95f);
        headerImg.raycastTarget = false;

        // Title label
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(header.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.sizeDelta = new Vector2(0f, 0f);
        titleRT.anchoredPosition = new Vector2(0f, 0f);
        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Photo Review";
        titleTMP.fontSize = 15f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.9f, 0.85f, 0.7f, 1f);
        titleTMP.raycastTarget = false;

        // Close button (X) in top-right corner
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(header.transform, false);
        RectTransform closeBtnRT = closeBtn.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1f, 0f);
        closeBtnRT.anchorMax = new Vector2(1f, 1f);
        closeBtnRT.pivot = new Vector2(1f, 0.5f);
        closeBtnRT.anchoredPosition = new Vector2(-6f, 0f);
        closeBtnRT.sizeDelta = new Vector2(80f, -6f);

        Image closeBtnImg = closeBtn.AddComponent<Image>();
        closeBtnImg.color = new Color(0.55f, 0.1f, 0.1f, 1f);

        Button closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.targetGraphic = closeBtnImg;
        ColorBlock cb = closeBtnComp.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 0.7f, 0.7f, 1f);
        cb.pressedColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        closeBtnComp.colors = cb;

        // Close button text
        GameObject closeTxtGO = new GameObject("Text");
        closeTxtGO.transform.SetParent(closeBtn.transform, false);
        RectTransform closeTxtRT = closeTxtGO.AddComponent<RectTransform>();
        closeTxtRT.anchorMin = Vector2.zero;
        closeTxtRT.anchorMax = Vector2.one;
        closeTxtRT.sizeDelta = Vector2.zero;
        closeTxtRT.anchoredPosition = Vector2.zero;
        TextMeshProUGUI closeTMP = closeTxtGO.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "✕ Close";
        closeTMP.fontSize = 13f;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;

        // Wire OnClick -> DaySummaryUI.TogglePhotoReview
        UnityAction toggleAction = System.Delegate.CreateDelegate(
            typeof(UnityAction), daySummaryUI, "TogglePhotoReview") as UnityAction;
        UnityEventTools.AddPersistentListener(closeBtnComp.onClick, toggleAction);
        EditorUtility.SetDirty(closeBtnComp);

        // ---- 4. Reposition PhotoGridParent to sit below the header ----
        if (photoGridParent != null)
        {
            RectTransform gridRT = photoGridParent.GetComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0f, 0f);
            gridRT.anchorMax = new Vector2(1f, 1f);
            gridRT.pivot = new Vector2(0.5f, 0.5f);
            gridRT.offsetMin = new Vector2(10f, 8f);    // left, bottom
            gridRT.offsetMax = new Vector2(-10f, -40f); // -right, -top (leave room for header)
        }

        // ---- 5. Also update "View Photos" button text to reflect toggle state ----
        // The button already calls TogglePhotoReview, which is fine.
        // Optionally rename it to make intent clearer:
        Transform card = daySummaryPanel.transform.Find("Card");
        if (card != null)
        {
            Transform btnContainer = card.Find("ButtonContainer");
            if (btnContainer != null)
            {
                Transform vpBtn = btnContainer.Find("ViewPhotosButton");
                if (vpBtn != null)
                {
                    TextMeshProUGUI vpTMP = vpBtn.GetComponentInChildren<TextMeshProUGUI>();
                    if (vpTMP != null) vpTMP.text = "View Photos";
                }
            }
        }

        Undo.RegisterCreatedObjectUndo(header, "Add PhotoReview Header");
        EditorUtility.SetDirty(daySummaryPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(daySummaryPanel.scene);
        Debug.Log("[FixPhotoReviewPanel] Done! raycastTarget fixed, close button added.");
    }
}
