using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    [Header("Overlay Settings")]
    [Tooltip("The fixed FOV for the jumpscare camera to avoid zoom issues.")]
    public float jumpscareFOV = 60f;

    [Header("Scare Effects")]
    public bool enableFlicker = true;
    public float minFlickerIntensity = 0.5f;
    public float maxFlickerIntensity = 5f;

    [Header("Backdrop Settings")]
    [Range(0f, 1f)]
    public float backdropOpacity = 0.85f;

    private AudioSource audioSource;
    private bool isJumpscareActive = false;
    private Camera jumpscareCamera;
    private int jumpscareLayer;
    private Light jumpscareLight;
    private GameObject backdropPlane;

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
        
        jumpscareLayer = LayerMask.NameToLayer("Jumpscare");
        if (jumpscareLayer == -1)
        {
            Debug.LogWarning("[JumpscareManager] 'Jumpscare' layer not found. Please create it in Tags and Layers.");
        }

        if (jumpscareContainer != null)
        {
            jumpscareContainer.SetActive(false);
            
            // Setup a light for the jumpscare
            GameObject lightObj = new GameObject("JumpscareLight");
            lightObj.transform.SetParent(jumpscareContainer.transform, false);
            lightObj.transform.localPosition = new Vector3(0, 1.8f, 0.8f); // Moved in front of the face
            
            jumpscareLight = lightObj.AddComponent<Light>();
            jumpscareLight.type = LightType.Point;
            jumpscareLight.color = Color.white;
            jumpscareLight.intensity = maxFlickerIntensity;
            jumpscareLight.range = 5f;
            jumpscareLight.gameObject.layer = jumpscareLayer;
        }

        SetupJumpscareCamera();
        SetupBackdrop();
    }

    private void SetupBackdrop()
    {
        if (jumpscareCamera == null || jumpscareLayer == -1) return;
        
        backdropPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backdropPlane.name = "JumpscareBackdrop";
        backdropPlane.transform.SetParent(jumpscareCamera.transform, false);
        
        // Position it behind the potential jumpscare man position
        backdropPlane.transform.localPosition = new Vector3(0, 0, 5f); 
        backdropPlane.transform.localScale = new Vector3(20, 20, 1);
        
        Destroy(backdropPlane.GetComponent<MeshCollider>());
        
        Renderer rend = backdropPlane.GetComponent<Renderer>();
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlitShader == null) unlitShader = Shader.Find("Unlit/Transparent");
        
        Material mat = new Material(unlitShader);
        mat.color = new Color(0, 0, 0, backdropOpacity);
        
        if (unlitShader.name.Contains("Universal"))
        {
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
        }
        
        rend.material = mat;
        backdropPlane.layer = jumpscareLayer;
        backdropPlane.SetActive(false);
    }

    private void SetupJumpscareCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null || jumpscareLayer == -1) return;

        // Ensure main camera ignores the Jumpscare layer
        mainCam.cullingMask &= ~(1 << jumpscareLayer);

        // Create overlay camera
        GameObject camObj = new GameObject("JumpscareOverlayCamera");
        camObj.transform.SetParent(mainCam.transform, false);
        
        jumpscareCamera = camObj.AddComponent<Camera>();
        jumpscareCamera.CopyFrom(mainCam);
        jumpscareCamera.cullingMask = 1 << jumpscareLayer;
        jumpscareCamera.fieldOfView = jumpscareFOV;
        jumpscareCamera.nearClipPlane = 0.01f; // Ensure it can see close objects
        
        var camData = jumpscareCamera.GetUniversalAdditionalCameraData();
        if (camData != null)
        {
            camData.renderType = CameraRenderType.Overlay;
        }
        
        var mainCamData = mainCam.GetUniversalAdditionalCameraData();
        if (mainCamData != null)
        {
            if (!mainCamData.cameraStack.Contains(jumpscareCamera))
            {
                mainCamData.cameraStack.Add(jumpscareCamera);
            }
        }
    }

    /// <summary>
    /// Triggers the jumpscare sequence and then shows the DaySummaryUI.
    /// </summary>
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

        // Force stop camera aiming/zoom and HIDE model
        CameraSystem camSystem = FindFirstObjectByType<CameraSystem>();
        if (camSystem != null)
        {
            camSystem.ForceStopAiming();
            camSystem.SetCameraModelVisible(false);
        }

        // 1. Activate the jumpscare visuals, backdrop and set layer
        if (backdropPlane != null) backdropPlane.SetActive(true);
        
        if (jumpscareContainer != null)
        {
            if (jumpscareLayer != -1)
            {
                SetLayerRecursively(jumpscareContainer, jumpscareLayer);
            }
            jumpscareContainer.SetActive(true);
        }

        // 2. Play the jumpscare sound
        if (jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound);
        }

        // 3. Flicker and Wait for the specified duration
        float elapsed = 0f;
        while (elapsed < jumpscareDuration)
        {
            if (enableFlicker && jumpscareLight != null)
            {
                // Random flicker ignoring timescale
                if (Random.value > 0.5f)
                    jumpscareLight.intensity = Random.Range(minFlickerIntensity, maxFlickerIntensity);
                else
                    jumpscareLight.intensity = 0f;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 4. Deactivate the jumpscare visuals and backdrop
        if (backdropPlane != null) backdropPlane.SetActive(false);
        
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

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
