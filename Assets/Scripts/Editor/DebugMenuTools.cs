using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class DebugMenuTools
{
    [MenuItem("Tools/Test Ending Scene")]
    public static void TestEndingScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Cannot use this while the game is already playing.");
            return;
        }

        // Open the Ending Scene in the Editor so that when we press Play, it starts here
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/EndingScene.unity");

        // We will hook into play mode start to inject stats
        EditorPrefs.SetBool("Debug_TestEnding", true);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Tools/Skip To Next Day")]
    public static void SkipToNextDay()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("You must be in Play Mode to skip to the next day.");
            return;
        }

        GameProgressionManager gpm = GameProgressionManager.Instance;
        if (gpm != null)
        {
            gpm.AdvanceDay();
            if (gpm.IsGameFinished)
            {
                SceneManager.LoadScene("EndingScene");
            }
            else
            {
                // Reload current scene to start the new day
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            Debug.Log("Skipped to Day " + gpm.currentDay);
        }
        else
        {
            Debug.LogError("GameProgressionManager not found!");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InjectStatsOnPlay()
    {
        if (EditorPrefs.GetBool("Debug_TestEnding", false))
        {
            EditorPrefs.SetBool("Debug_TestEnding", false); // consume

            // Only inject if the loaded scene is EndingScene (or will be)
            // BeforeSceneLoad runs right before the first scene loads, so SceneManager.GetActiveScene() should be EndingScene
            if (SceneManager.GetActiveScene().name == "EndingScene")
            {
                InjectStats();
            }
        }
    }

    private static void InjectStats()
    {
        // Try to find an existing GameProgressionManager, or create a new one
        GameProgressionManager gpm = Object.FindFirstObjectByType<GameProgressionManager>();
        if (gpm == null)
        {
            GameObject go = new GameObject("GameProgressionManager");
            gpm = go.AddComponent<GameProgressionManager>();
        }

        // Add dummy stats to simulate a completed game
        gpm.ResetProgression();
        gpm.currentDay = gpm.maxDays;
        gpm.AddDailyStats(18, 20, new System.Collections.Generic.List<Texture2D>());

        Debug.Log("Injected test stats into GameProgressionManager for EndingScene.");
    }
}
