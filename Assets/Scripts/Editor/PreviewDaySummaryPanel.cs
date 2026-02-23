using UnityEngine;
using UnityEditor;

public class PreviewDaySummaryPanel
{
    [MenuItem("Tools/Preview DaySummary Panel (Toggle)")]
    public static void Execute()
    {
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allObjects)
        {
            if (go.name == "DaySummaryPanel" && go.scene.IsValid())
            {
                bool current = go.activeSelf;
                go.SetActive(!current);
                Debug.Log($"[Preview] DaySummaryPanel set active: {!current}");
                break;
            }
        }
    }
}
