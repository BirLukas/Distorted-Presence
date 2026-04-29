using UnityEngine;

public class AtmosphereManager : MonoBehaviour
{
    public static AtmosphereManager Instance { get; private set; }

    [Header("Ambient (Basic atmosphere)")]
    public AudioClip[] ambientTracks;
    [Range(0f, 1f)] public float ambientVolume = 0.5f;
    public bool playAmbientOnStart = true;
    
    [Header("Whispers (Random sounds)")]
    public AudioClip[] whisperClips;
    [Range(0f, 1f)] public float whisperVolume = 1f;
    public float whisperMinDelay = 15f;
    public float whisperMaxDelay = 40f;

    [Header("Effect of sanity (Sanity)")]
    public bool scaleWithSanity = true;
    [Tooltip("If sanity is 0, the whisper interval is multiplied by this number (e.g., 0.3 means they will sound more than 3 times more often).")]
    [Range(0.1f, 1f)] public float lowSanityDelayMultiplier = 0.3f;

    private AudioSource ambientSource;
    private AudioSource whisperSource;
    private float whisperTimer;

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

    private void Start()
    {
        // Pokusíme se najít existující AudioSource (např. ten, co už na AmbientManageru máš)
        ambientSource = GetComponent<AudioSource>();
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.spatialBlend = 0f; 
        }

        // Šepoty dostanou svůj vlastní nezávislý AudioSource
        whisperSource = gameObject.AddComponent<AudioSource>();
        whisperSource.loop = false;
        whisperSource.volume = whisperVolume;
        whisperSource.spatialBlend = 0.5f;

        // Pokud jsi do skriptu přiřadil další ambienty, budeme je střídat. 
        // Pokud ne, necháme hrát ten, co už máš nastavený přímo v komponentě.
        if (ambientTracks != null && ambientTracks.Length > 0)
        {
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            if (playAmbientOnStart) PlayRandomAmbient();
        }

        ResetWhisperTimer();
    }

    private void Update()
    {
        if (whisperClips.Length > 0)
        {
            whisperTimer -= Time.deltaTime;
            if (whisperTimer <= 0f)
            {
                PlayRandomWhisper();
                ResetWhisperTimer();
            }
        }
    }

    public void PlayRandomAmbient()
    {
        if (ambientTracks.Length == 0) return;
        int index = Random.Range(0, ambientTracks.Length);
        ambientSource.clip = ambientTracks[index];
        ambientSource.Play();
    }

    public void PlayRandomWhisper()
    {
        if (whisperClips.Length == 0) return;
        int index = Random.Range(0, whisperClips.Length);
        
        // Mírná náhodnost ve výšce tónu (pitch) pro větší variabilitu
        whisperSource.pitch = Random.Range(0.9f, 1.1f);
        whisperSource.PlayOneShot(whisperClips[index], whisperVolume);
    }

    private void ResetWhisperTimer()
    {
        float delay = Random.Range(whisperMinDelay, whisperMaxDelay);

        if (scaleWithSanity && SanityManager.Instance != null)
        {
            // Sanity je od 0 do 100
            float sanityNorm = Mathf.Clamp01(SanityManager.Instance.CurrentSanity / 100f);
            
            // Pokud je sanity 1 (100%), multiplier je 1f. Pokud je 0, multiplier je lowSanityDelayMultiplier.
            float multiplier = Mathf.Lerp(lowSanityDelayMultiplier, 1f, sanityNorm);
            delay *= multiplier;
        }

        whisperTimer = delay;
    }
}
