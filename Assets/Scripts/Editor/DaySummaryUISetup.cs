using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

public class DaySummaryUISetup
{
    [MenuItem("Tools/Setup DaySummaryUI")]
    public static void Execute()
    {
        // ── 1. Find DaySummaryPanel (may be inactive, so search all objects) ─
        GameObject daySummaryPanel = null;
        foreach (DaySummaryUI comp in Resources.FindObjectsOfTypeAll<DaySummaryUI>())
        {
            // Only consider scene objects, not prefab assets
            if (comp.gameObject.scene.IsValid())
            {
                daySummaryPanel = comp.gameObject;
                break;
            }
        }

        if (daySummaryPanel == null)
        {
            Debug.LogError("[DaySummaryUISetup] Could not find DaySummaryPanel (with DaySummaryUI component) in the scene.");
            return;
        }

        DaySummaryUI daySummaryUI = daySummaryPanel.GetComponent<DaySummaryUI>();
        if (daySummaryUI == null)
        {
            Debug.LogError("[DaySummaryUISetup] DaySummaryPanel does not have a DaySummaryUI component.");
            return;
        }

        // Get the Card child (where existing texts live)
        Transform card = daySummaryPanel.transform.Find("Card");
        if (card == null)
        {
            Debug.LogError("[DaySummaryUISetup] Could not find 'Card' child inside DaySummaryPanel.");
            return;
        }

        // ── 2. Create DaysLeftText ───────────────────────────────────────────
        // Check if it already exists
        Transform existingDaysLeft = card.Find("DaysLeftText");
        GameObject daysLeftGO;
        if (existingDaysLeft != null)
        {
            daysLeftGO = existingDaysLeft.gameObject;
            Debug.Log("[DaySummaryUISetup] DaysLeftText already exists, reusing it.");
        }
        else
        {
            daysLeftGO = new GameObject("DaysLeftText");
            daysLeftGO.transform.SetParent(card, false);

            // Add TMP component
            TextMeshProUGUI tmp = daysLeftGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Day <b>1</b> / 5";
            tmp.fontSize = 18f;
            tmp.fontStyle = FontStyles.Normal;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            // Position it just below TitleText (anchored top, full width)
            RectTransform rt = daysLeftGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -82f); // below TitleText (-10 offset + 70 height + 2 gap)
            rt.sizeDelta = new Vector2(0f, 30f);

            Debug.Log("[DaySummaryUISetup] Created DaysLeftText.");
        }

        // Assign to DaySummaryUI
        TextMeshProUGUI daysLeftTMP = daysLeftGO.GetComponent<TextMeshProUGUI>();
        daySummaryUI.daysLeftText = daysLeftTMP;

        // ── 3. Create PhotoGridParent ────────────────────────────────────────
        Transform existingGrid = daySummaryPanel.transform.Find("PhotoGridParent");
        GameObject photoGridGO;
        if (existingGrid != null)
        {
            photoGridGO = existingGrid.gameObject;
            Debug.Log("[DaySummaryUISetup] PhotoGridParent already exists, reusing it.");
        }
        else
        {
            photoGridGO = new GameObject("PhotoGridParent");
            photoGridGO.transform.SetParent(daySummaryPanel.transform, false);

            // RectTransform – bottom half of the panel, full width with padding
            RectTransform rt = photoGridGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 20f);
            rt.sizeDelta = new Vector2(-40f, 200f);

            // GridLayoutGroup
            GridLayoutGroup grid = photoGridGO.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120f, 90f);
            grid.spacing = new Vector2(10f, 10f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            Debug.Log("[DaySummaryUISetup] Created PhotoGridParent with GridLayoutGroup.");
        }

        // Assign to DaySummaryUI
        daySummaryUI.photoGridParent = photoGridGO.transform;

        // ── 4. Create PhotoReviewPrefab ──────────────────────────────────────
        string prefabDir = "Assets/Prefabs";
        string prefabPath = prefabDir + "/PhotoReviewPrefab.prefab";

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder(prefabDir))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Build the prefab in memory
        GameObject prefabRoot = new GameObject("PhotoReviewPrefab");
        prefabRoot.AddComponent<RectTransform>();

        // RawImage on root
        RawImage rawImage = prefabRoot.AddComponent<RawImage>();
        rawImage.color = Color.white;

        // Set root RectTransform to fill its cell
        RectTransform rootRT = prefabRoot.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;
        rootRT.anchoredPosition = Vector2.zero;

        // ── 5. ResultIcon child ──────────────────────────────────────────────
        GameObject resultIconGO = new GameObject("ResultIcon");
        resultIconGO.transform.SetParent(prefabRoot.transform, false);

        Image resultIconImage = resultIconGO.AddComponent<Image>();
        resultIconImage.color = Color.white;
        resultIconImage.preserveAspect = true;

        RectTransform iconRT = resultIconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(1f, 1f);
        iconRT.anchorMax = new Vector2(1f, 1f);
        iconRT.pivot = new Vector2(1f, 1f);
        iconRT.anchoredPosition = new Vector2(-4f, -4f);
        iconRT.sizeDelta = new Vector2(28f, 28f);

        // ── 6. Attach PhotoReviewUI script ───────────────────────────────────
        PhotoReviewUI photoReviewUI = prefabRoot.AddComponent<PhotoReviewUI>();
        photoReviewUI.photoImage = rawImage;
        photoReviewUI.resultIcon = resultIconImage;

        // ── 7. Generate checkmark and cross sprites ──────────────────────────
        Sprite checkSprite = CreateCheckmarkSprite();
        Sprite crossSprite = CreateCrossSprite();

        photoReviewUI.correctSprite = checkSprite;
        photoReviewUI.incorrectSprite = crossSprite;

        // ── 8. Save as prefab ────────────────────────────────────────────────
        bool prefabSuccess;
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath, out prefabSuccess);
        Object.DestroyImmediate(prefabRoot);

        if (!prefabSuccess || savedPrefab == null)
        {
            Debug.LogError("[DaySummaryUISetup] Failed to save PhotoReviewPrefab.");
            return;
        }

        // Reload the saved prefab to get the correct asset reference
        GameObject photoPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // Assign sprites to the saved prefab's PhotoReviewUI
        PhotoReviewUI savedPhotoReviewUI = photoPrefabAsset.GetComponent<PhotoReviewUI>();
        if (savedPhotoReviewUI != null)
        {
            savedPhotoReviewUI.correctSprite = checkSprite;
            savedPhotoReviewUI.incorrectSprite = crossSprite;
            EditorUtility.SetDirty(photoPrefabAsset);
            AssetDatabase.SaveAssets();
        }

        // ── 9. Assign photoPrefab to DaySummaryUI ────────────────────────────
        daySummaryUI.photoPrefab = photoPrefabAsset;

        // Mark scene dirty
        EditorUtility.SetDirty(daySummaryPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        AssetDatabase.Refresh();

        Debug.Log("[DaySummaryUISetup] ✅ Setup complete! DaysLeftText, PhotoGridParent, and PhotoReviewPrefab are all configured.");
    }

    // ── Sprite generation helpers ────────────────────────────────────────────

    private static Sprite CreateCheckmarkSprite()
    {
        string path = "Assets/Textures/UI/checkmark.png";
        EnsureUITextureDir();

        Texture2D tex = DrawCheckmark(64, 64);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite CreateCrossSprite()
    {
        string path = "Assets/Textures/UI/cross.png";
        EnsureUITextureDir();

        Texture2D tex = DrawCross(64, 64);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void EnsureUITextureDir()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Textures"))
            AssetDatabase.CreateFolder("Assets", "Textures");
        if (!AssetDatabase.IsValidFolder("Assets/Textures/UI"))
            AssetDatabase.CreateFolder("Assets/Textures", "UI");
    }

    private static Texture2D DrawCheckmark(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color green = new Color(0.2f, 0.85f, 0.2f, 1f);

        // Fill transparent
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        // Draw a thick checkmark: two line segments
        // Segment 1: (10,30) -> (26,14)  (left leg going down-left to center)
        // Segment 2: (26,14) -> (54,46)  (right leg going up-right)
        DrawThickLine(tex, 10, 30, 26, 14, 5, green);
        DrawThickLine(tex, 26, 14, 54, 46, 5, green);

        tex.Apply();
        return tex;
    }

    private static Texture2D DrawCross(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color red = new Color(0.9f, 0.15f, 0.15f, 1f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        int pad = 10;
        DrawThickLine(tex, pad, pad, w - pad, h - pad, 5, red);
        DrawThickLine(tex, w - pad, pad, pad, h - pad, 5, red);

        tex.Apply();
        return tex;
    }

    private static void DrawThickLine(Texture2D tex, int x0, int y0, int x1, int y1, int thickness, Color col)
    {
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        int half = thickness / 2;

        while (true)
        {
            for (int ox = -half; ox <= half; ox++)
                for (int oy = -half; oy <= half; oy++)
                {
                    int px = x0 + ox, py = y0 + oy;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        tex.SetPixel(px, py, col);
                }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}
