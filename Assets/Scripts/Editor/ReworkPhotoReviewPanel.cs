using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using TMPro;

public class ReworkPhotoReviewPanel
{
    public static void Execute()
    {
        // Find objects (including inactive)
        GameObject canvasUI = null;
        GameObject daySummaryPanel = null;

        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.IsValid()) continue;
            if (go.name == "Canvas_UI") canvasUI = go;
            if (go.name == "DaySummaryPanel") daySummaryPanel = go;
        }

        if (canvasUI == null) { Debug.LogError("Canvas_UI not found!"); return; }
        if (daySummaryPanel == null) { Debug.LogError("DaySummaryPanel not found!"); return; }

        DaySummaryUI daySummaryUI = daySummaryPanel.GetComponent<DaySummaryUI>();
        if (daySummaryUI == null) { Debug.LogError("DaySummaryUI not found!"); return; }

        // ---- 1. Destroy old PhotoReviewPanel inside DaySummaryPanel ----
        Transform oldPanel = daySummaryPanel.transform.Find("PhotoReviewPanel");
        Transform savedGrid = null;
        if (oldPanel != null)
        {
            savedGrid = oldPanel.Find("PhotoGridParent");
            if (savedGrid != null) savedGrid.SetParent(canvasUI.transform, false); // temp reparent
            Object.DestroyImmediate(oldPanel.gameObject);
        }

        // ---- 2. Create new PhotoReviewPanel as child of Canvas_UI (full-screen overlay) ----
        GameObject photoReviewPanel = new GameObject("PhotoReviewPanel");
        photoReviewPanel.transform.SetParent(canvasUI.transform, false);

        // Place it after DaySummaryPanel in sibling order (renders on top)
        int daySummaryIndex = daySummaryPanel.transform.GetSiblingIndex();
        photoReviewPanel.transform.SetSiblingIndex(daySummaryIndex + 1);

        RectTransform prRT = photoReviewPanel.AddComponent<RectTransform>();
        prRT.anchorMin = Vector2.zero;
        prRT.anchorMax = Vector2.one;
        prRT.offsetMin = Vector2.zero;
        prRT.offsetMax = Vector2.zero;

        // Semi-transparent dark overlay background
        Image bgImg = photoReviewPanel.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.88f);
        bgImg.raycastTarget = true; // blocks clicks on world behind it

        // ---- 3. Inner content box ----
        GameObject contentBox = new GameObject("ContentBox");
        contentBox.transform.SetParent(photoReviewPanel.transform, false);
        RectTransform cbRT = contentBox.AddComponent<RectTransform>();
        cbRT.anchorMin = new Vector2(0.05f, 0.08f);
        cbRT.anchorMax = new Vector2(0.95f, 0.92f);
        cbRT.offsetMin = Vector2.zero;
        cbRT.offsetMax = Vector2.zero;

        Image cbImg = contentBox.AddComponent<Image>();
        cbImg.color = new Color(0.07f, 0.05f, 0.03f, 0.98f);
        cbImg.raycastTarget = false;

        // ---- 4. Header bar ----
        GameObject header = new GameObject("Header");
        header.transform.SetParent(contentBox.transform, false);
        RectTransform headerRT = header.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0f, 1f);
        headerRT.anchorMax = new Vector2(1f, 1f);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = Vector2.zero;
        headerRT.sizeDelta = new Vector2(0f, 48f);

        Image headerImg = header.AddComponent<Image>();
        headerImg.color = new Color(0.12f, 0.09f, 0.06f, 1f);
        headerImg.raycastTarget = false;

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(header.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = Vector2.zero;
        titleRT.anchorMax = Vector2.one;
        titleRT.offsetMin = new Vector2(10f, 0f);
        titleRT.offsetMax = new Vector2(-110f, 0f);
        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "📷  Photo Review";
        titleTMP.fontSize = 18f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
        titleTMP.color = new Color(0.9f, 0.85f, 0.65f, 1f);
        titleTMP.raycastTarget = false;

        // Close button
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(header.transform, false);
        RectTransform closeBtnRT = closeBtn.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1f, 0f);
        closeBtnRT.anchorMax = new Vector2(1f, 1f);
        closeBtnRT.pivot = new Vector2(1f, 0.5f);
        closeBtnRT.anchoredPosition = new Vector2(-8f, 0f);
        closeBtnRT.sizeDelta = new Vector2(100f, -8f);

        Image closeBtnImg = closeBtn.AddComponent<Image>();
        closeBtnImg.color = new Color(0.6f, 0.1f, 0.1f, 1f);

        Button closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.targetGraphic = closeBtnImg;
        ColorBlock cb = closeBtnComp.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 0.6f, 0.6f, 1f);
        cb.pressedColor = new Color(0.7f, 0.2f, 0.2f, 1f);
        closeBtnComp.colors = cb;

        GameObject closeTxtGO = new GameObject("Text");
        closeTxtGO.transform.SetParent(closeBtn.transform, false);
        RectTransform closeTxtRT = closeTxtGO.AddComponent<RectTransform>();
        closeTxtRT.anchorMin = Vector2.zero;
        closeTxtRT.anchorMax = Vector2.one;
        closeTxtRT.sizeDelta = Vector2.zero;
        closeTxtRT.anchoredPosition = Vector2.zero;
        TextMeshProUGUI closeTMP = closeTxtGO.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "✕  Close";
        closeTMP.fontSize = 14f;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;

        // Wire close button -> DaySummaryUI.TogglePhotoReview
        UnityAction toggleAction = System.Delegate.CreateDelegate(
            typeof(UnityAction), daySummaryUI, "TogglePhotoReview") as UnityAction;
        UnityEventTools.AddPersistentListener(closeBtnComp.onClick, toggleAction);
        EditorUtility.SetDirty(closeBtnComp);

        // ---- 5. ScrollView for photos ----
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(contentBox.transform, false);
        RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(10f, 10f);
        scrollRT.offsetMax = new Vector2(-10f, -52f); // leave room for header

        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30f;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform vpRT = viewport.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.01f);
        scrollRect.viewport = vpRT;

        // ---- 6. Move or recreate PhotoGridParent inside viewport ----
        GameObject photoGridParent;
        if (savedGrid != null)
        {
            photoGridParent = savedGrid.gameObject;
            photoGridParent.transform.SetParent(viewport.transform, false);
        }
        else
        {
            photoGridParent = new GameObject("PhotoGridParent");
            photoGridParent.transform.SetParent(viewport.transform, false);
            photoGridParent.AddComponent<GridLayoutGroup>();
        }

        RectTransform gridRT = photoGridParent.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0f, 1f);
        gridRT.anchorMax = new Vector2(1f, 1f);
        gridRT.pivot = new Vector2(0.5f, 1f);
        gridRT.anchoredPosition = new Vector2(0f, 0f);
        gridRT.sizeDelta = new Vector2(0f, 400f);

        GridLayoutGroup glg = photoGridParent.GetComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(160f, 120f);
        glg.spacing = new Vector2(12f, 12f);
        glg.padding = new RectOffset(12, 12, 12, 12);
        glg.constraint = GridLayoutGroup.Constraint.Flexible;
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment = TextAnchor.UpperLeft;

        // ContentSizeFitter so grid grows with content
        ContentSizeFitter csf = photoGridParent.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = photoGridParent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scrollRect.content = gridRT;

        // ---- 7. Update DaySummaryUI references ----
        Undo.RecordObject(daySummaryUI, "Rework PhotoReviewPanel");
        daySummaryUI.photoReviewPanel = photoReviewPanel;
        daySummaryUI.photoGridParent = photoGridParent.transform;
        EditorUtility.SetDirty(daySummaryUI);

        // Start hidden
        photoReviewPanel.SetActive(false);

        Undo.RegisterCreatedObjectUndo(photoReviewPanel, "Create PhotoReviewPanel overlay");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvasUI.scene);
        Debug.Log("[ReworkPhotoReviewPanel] Done! Full-screen overlay PhotoReviewPanel created.");
    }
}
