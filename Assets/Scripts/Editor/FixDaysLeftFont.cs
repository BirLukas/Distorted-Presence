using UnityEngine;
using UnityEditor;
using TMPro;

public class FixDaysLeftFont
{
    [MenuItem("Tools/Fix DaysLeftText Font")]
    public static void Execute()
    {
        // Load the font asset used by the rest of the UI
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        if (font == null)
        {
            Debug.LogError("[FixDaysLeftFont] Could not load LiberationSans SDF font asset.");
            return;
        }

        // Find DaysLeftText (may be inactive)
        TextMeshProUGUI daysLeftTMP = null;
        foreach (TextMeshProUGUI tmp in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (tmp.gameObject.name == "DaysLeftText" && tmp.gameObject.scene.IsValid())
            {
                daysLeftTMP = tmp;
                break;
            }
        }

        if (daysLeftTMP == null)
        {
            Debug.LogError("[FixDaysLeftFont] Could not find DaysLeftText in the scene.");
            return;
        }

        daysLeftTMP.font = font;
        daysLeftTMP.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        daysLeftTMP.fontSize = 18f;
        daysLeftTMP.alignment = TextAlignmentOptions.Center;

        EditorUtility.SetDirty(daysLeftTMP);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[FixDaysLeftFont] ✅ Font assigned to DaysLeftText successfully.");
    }
}
