using UnityEngine;
using UnityEngine.Events;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    [Header("Settings")]
    [Range(0f, 100f)]
    private float currentSanity = 100f;
    public float CurrentSanity => currentSanity;

    public float correctAnomalyBonus = 5f;
    public float incorrectAnomalyPenalty = 10f;
    public float unreportedAnomalyPenalty = 1f;

    [Header("Events & UI")]
    public UnityEvent OnSanityZero;
    public UnityEvent OnVictory;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddSanity(float amount)
    {
        if (isGameOver) return;
        currentSanity = Mathf.Clamp(currentSanity + amount, 0f, 100f);
        CheckForLossCondition();
    }

    public void RemoveSanity(float amount)
    {
        if (isGameOver) return;
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, 100f);
        CheckForLossCondition();
    }

    public void ApplyUnreportedPenalty(int unreportedCount)
    {
        if (isGameOver) return;
        float penalty = unreportedCount * unreportedAnomalyPenalty * Time.deltaTime;
        RemoveSanity(penalty);
    }

    private void CheckForLossCondition()
    {
        if (isGameOver) return;
        if (currentSanity <= 0.001f)
        {
            currentSanity = 0f;
            isGameOver = true;
            OnSanityZero.Invoke();
        }
    }
    public void Victory()
    {
        if (isGameOver) return;
        isGameOver = true;
        OnVictory.Invoke();
    }

    public void ResetSanity(float initialValue = 100f)
    {
        currentSanity = initialValue;
        isGameOver = false;
    }
}
