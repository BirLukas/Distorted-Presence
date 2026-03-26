using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DistortedSound : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioDistortionFilter distortionFilter;

    [Header("Nastavení zkreslení audia")]
    [Range(0f, 1f)]
    [Tooltip("Intenzita audio filtru")]
    public float distortionLevel = 0.8f;
    
    [Tooltip("Minimální hodnota přehrávací rychlosti (tón hlasu)")]
    public float minPitch = 0.6f;
    [Tooltip("Maximální hodnota přehrávací rychlosti")]
    public float maxPitch = 0.9f;
    [Tooltip("Jak rychle se pitch bude měnit")]
    public float pitchChangeSpeed = 2f;

    private float originalPitch;
    private float targetPitch;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        originalPitch = audioSource.pitch;

        distortionFilter = GetComponent<AudioDistortionFilter>();
        if (distortionFilter == null)
        {
            distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        }

        distortionFilter.distortionLevel = distortionLevel;
        targetPitch = Random.Range(minPitch, maxPitch) * originalPitch;
    }

    void Update()
    {
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * pitchChangeSpeed);
        
        if (Mathf.Abs(audioSource.pitch - targetPitch) < 0.05f)
        {
            targetPitch = Random.Range(minPitch, maxPitch) * originalPitch;
        }
    }

    public void StopDistortion()
    {
        if (distortionFilter != null)
        {
            distortionFilter.distortionLevel = 0f;
        }
        audioSource.pitch = originalPitch;
        this.enabled = false;
    }
}
