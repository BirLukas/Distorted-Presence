using UnityEngine;

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance { get; private set; }

    [Header("Day Progression")]
    public int currentDay = 1;
    public int maxDays = 5;

    public bool IsGameFinished { get; private set; }

    [Header("Difficulty Scaling")]
    public float baseMinGeneratedDelay = 10f;
    public float baseMaxGeneratedDelay = 20f;
    public float baseSanityPenalty = 1f;
    
    // Obdrzi vysledek snizeni delaye pro dany den
    public float CurrentMinDelay => Mathf.Max(2f, baseMinGeneratedDelay - (currentDay - 1) * 1.5f);
    public float CurrentMaxDelay => Mathf.Max(5f, baseMaxGeneratedDelay - (currentDay - 1) * 2f);
    
    // Obdrzi zvysenou penalizaci za minule / propasnute anomalie podle dne
    public float CurrentSanityPenalty => baseSanityPenalty + (currentDay - 1) * 0.5f;

    // Kolik anomalii by melo dany den maximalne vzniknout.
    // Lze nastavit i jinak, udelame ze napr. den 1 = 3 anomalie, den 5 = 7 anomalii
    public int MaxAnomaliesPerDay => 2 + currentDay;

    [Header("Playthrough History")]
    public System.Collections.Generic.List<string> activatedAnomalyNames = new System.Collections.Generic.List<string>();

    [Header("Global Stats (Ending)")]
    public int globalPhotographed = 0;
    public int globalTriggered = 0;
    public System.Collections.Generic.List<Texture2D> globalPhotos = new System.Collections.Generic.List<Texture2D>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void AddDailyStats(int photographed, int triggered, System.Collections.Generic.List<Texture2D> photosToKeep)
    {
        globalPhotographed += photographed;
        globalTriggered += triggered;
        
        if (photosToKeep != null)
        {
            foreach(var p in photosToKeep)
            {
                // We keep up to 15 photos globally to avoid massive memory usage
                if (globalPhotos.Count < 15)
                {
                    globalPhotos.Add(p);
                }
            }
        }
    }

    public void AdvanceDay()
    {
        if (currentDay < maxDays)
        {
            currentDay++;
            Debug.Log($"Advanced to Day {currentDay}");
        }
        else
        {
            Debug.Log("Game Finished! All 5 days completed.");
            IsGameFinished = true;
        }
    }

    public void ResetProgression()
    {
        currentDay = 1;
        IsGameFinished = false;
        activatedAnomalyNames.Clear();
        
        globalPhotographed = 0;
        globalTriggered = 0;
        globalPhotos.Clear();
    }
}
