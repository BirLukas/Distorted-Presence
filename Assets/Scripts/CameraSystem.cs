using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class CameraSystem : MonoBehaviour
{
    [Header("Camera Objects")]
    public GameObject cameraModel;
    public GameObject cameraUI;
    public CanvasGroup flashGroup;
    public Camera playerCamera;
    
    // Nová překryvná kamera pro zbraň/foťák
    private Camera overlayCamera;

    [Header("Camera Setup")]
    public float defaultFOV = 60f;
    public float aimedFOV = 30f;
    public float aimSpeed = 5f;

    [Header("Flash Setup")]
    public float flashDuration = 0.1f;

    private PhotoCapture photoCapture;
    private bool isAimed = false;
    public static bool IsAimingGlobal { get; private set; }

    void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera is not assigned!");
            return;
        }

        photoCapture = playerCamera.GetComponent<PhotoCapture>();

        if (cameraUI != null) cameraUI.SetActive(false);
        if (flashGroup != null) flashGroup.alpha = 0f;

        playerCamera.fieldOfView = defaultFOV;
        
        SetupOverlayCamera();
    }

    void SetupOverlayCamera()
    {
        if (cameraModel == null || playerCamera == null) return;

        // Vrstva 7 by u tebe měla být "CameraModel", pokud není, script nic nerozbije
        int layerCameraModel = LayerMask.NameToLayer("CameraModel");
        if (layerCameraModel == -1)
        {
            Debug.LogWarning("Vrstva 'CameraModel' neexistuje. Vytvořte ji v Tags and Layers pro zamezení prolínání zbraně do zdí.");
            return;
        }

        // Vytvoříme novou kameru dynamicky
        GameObject overlayCamObj = new GameObject("ViewModelCamera");
        overlayCamObj.transform.SetParent(playerCamera.transform, false);
        
        overlayCamera = overlayCamObj.AddComponent<Camera>();
        overlayCamera.CopyFrom(playerCamera);
        
        // Nastavíme vrstvy, aby se obě kamery nepletly do cesty
        playerCamera.cullingMask &= ~(1 << layerCameraModel); // Hlavní kamera už kameraman neuvidí
        overlayCamera.cullingMask = 1 << layerCameraModel;    // Overlay kamera uvidí JENOM kameraman
        
        // Ujistíme se, že předmět v ruce má správnou vrstvu
        SetLayerRecursively(cameraModel, layerCameraModel);

        // Nastavíme překrývání URP (Zajistí render nad vrstvou prostředí, i když fyzicky foťák je ve zdi)
        var overlayCamData = overlayCamera.GetUniversalAdditionalCameraData();
        if (overlayCamData != null)
        {
            overlayCamData.renderType = CameraRenderType.Overlay;
        }

        var baseCamData = playerCamera.GetUniversalAdditionalCameraData();
        if (baseCamData != null)
        {
            if (!baseCamData.cameraStack.Contains(overlayCamera))
            {
                baseCamData.cameraStack.Add(overlayCamera);
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void OnAim(InputValue value)
    {
        if (SanityManager.Instance != null && SanityManager.Instance.IsGameOver)
        {
            if (isAimed) StopAiming();
            isAimed = false;
            return;
        }

        isAimed = value.isPressed;

        if (isAimed) AimCamera();
        else StopAiming();
    }

    public void OnFire(InputValue value)
    {
        if (SanityManager.Instance != null && SanityManager.Instance.IsGameOver) return;

        if (value.isPressed && isAimed)
        {
            TakePhoto();
        }
    }

    void Update()
    {
        float targetFOV = isAimed ? aimedFOV : defaultFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);
        
        // Dynamický přenos FOV i na zbraňovou kameru
        if (overlayCamera != null)
        {
            overlayCamera.fieldOfView = playerCamera.fieldOfView;
        }

        IsAimingGlobal = isAimed;
    }

    void AimCamera()
    {
        if (cameraModel != null) cameraModel.SetActive(false);
        if (cameraUI != null) cameraUI.SetActive(true);
    }

    void StopAiming()
    {
        if (cameraModel != null) cameraModel.SetActive(true);
        if (cameraUI != null) cameraUI.SetActive(false);
    }

    void TakePhoto()
    {
        if (photoCapture != null)
        {
            if (photoCapture.GetRemainingFilmCount() > 0)
            {
                photoCapture.TryCaptureAnomaly(() => 
                {
                    StartCoroutine(FlashEffect());
                });
            }
            else
            {
                photoCapture.TryCaptureAnomaly();
            }
        }
    }

    IEnumerator FlashEffect()
    {
        if (flashGroup != null) flashGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(flashDuration);
        if (flashGroup != null) flashGroup.alpha = 0f;
    }

    public void ForceStopAiming()
    {
        isAimed = false;
        IsAimingGlobal = false;
        
        if (cameraUI != null) cameraUI.SetActive(false);
        
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = defaultFOV;
        }
        
        if (overlayCamera != null)
        {
            overlayCamera.fieldOfView = defaultFOV;
        }
    }

    public void SetCameraModelVisible(bool visible)
    {
        if (cameraModel != null) cameraModel.SetActive(visible);
    }
}
