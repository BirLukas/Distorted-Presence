using System.Collections;
using UnityEngine;

public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance { get; private set; }

    [Header("Jumpscare Settings")]
    [Tooltip("The GameObject to enable during the jumpscare. Can be a UI Panel, a 3D monster, etc.")]
    public GameObject jumpscareContainer;
    
    [Tooltip("The sound to play during the jumpscare.")]
    public AudioClip jumpscareSound;
    
    [Tooltip("How long the jumpscare lasts before showing the summary.")]
    public float jumpscareDuration = 4f;

    private AudioSource audioSource;
    private bool isJumpscareActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // This ensures the audio can play even if the game is paused (timeScale = 0)
        audioSource.ignoreListenerPause = true;
        
        if (jumpscareContainer != null)
        {
            jumpscareContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Triggers the jumpscare sequence and then shows the DaySummaryUI.
    /// </summary>
    /// <param name="sanity">Current sanity to pass to the summary</param>
    /// <param name="photographed">Number of photographed anomalies</param>
    /// <param name="triggered">Total number of triggered anomalies</param>
    /// <param name="reason">Reason for losing</param>
    public void TriggerJumpscare(float sanity, int photographed, int triggered, string reason)
    {
        if (isJumpscareActive) return;
        
        StartCoroutine(JumpscareRoutine(sanity, photographed, triggered, reason));
    }

    private IEnumerator JumpscareRoutine(float sanity, int photographed, int triggered, string reason)
    {
        isJumpscareActive = true;

        // Ensure time is stopped
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 1. Activate the jumpscare visuals
        if (jumpscareContainer != null)
        {
            jumpscareContainer.SetActive(true);
        }

        // 2. Play the jumpscare sound
        if (jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound);
        }

        // 3. Wait for the specified duration (using Realtime since timeScale is 0)
        yield return new WaitForSecondsRealtime(jumpscareDuration);

        // 4. Deactivate the jumpscare visuals
        if (jumpscareContainer != null)
        {
            jumpscareContainer.SetActive(false);
        }

        // 5. Show the DaySummaryUI
        DaySummaryUI ui = DaySummaryUI.Instance;
        if (ui == null)
        {
            ui = FindFirstObjectByType<DaySummaryUI>(FindObjectsInactive.Include);
        }

        if (ui != null)
        {
            ui.ShowSummary(false, sanity, photographed, triggered, reason);
        }
        else
        {
            Debug.LogError("[JumpscareManager] DaySummaryUI not found in scene!");
        }

        isJumpscareActive = false;
    }
}
