using UnityEngine;

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance { get; private set; }

    [Header("Day Progression")]
    public int currentDay = 1;
    public int maxDays = 5;

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
            // TBD Vitezstvi
        }
    }

    public void ResetProgression()
    {
        currentDay = 1;
    }
}
