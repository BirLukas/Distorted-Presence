using UnityEngine;
using UnityEditor;

public class PreviewPhotoReviewPanel
{
    public static void Execute()
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "PhotoReviewPanel" && go.scene.IsValid())
            {
                go.SetActive(true);
                Debug.Log("[Preview] PhotoReviewPanel activated for preview.");
                break;
            }
        }
    }
}
