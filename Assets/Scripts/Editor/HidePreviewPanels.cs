using UnityEngine;
using UnityEditor;

public class HidePreviewPanels
{
    public static void Execute()
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.IsValid()) continue;
            if (go.name == "DaySummaryPanel" || go.name == "PhotoReviewPanel")
            {
                go.SetActive(false);
                Debug.Log($"[HidePreview] {go.name} hidden.");
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
