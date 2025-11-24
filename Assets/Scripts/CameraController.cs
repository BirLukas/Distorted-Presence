using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Reference")]
    public Camera playerCamera;      // Main Camera
    public Camera photoCamera;       // Photo Camera (disabled on start)
    public Transform cameraModel;    // 3D model fotoaparátu
    public Transform restPosition;   // pozice v ruce
    public Transform aimPosition;    // pozice u oka
    public CanvasGroup photoUI;      // rámeèek a UI zobrazované pøi míøení
    public CanvasGroup flashEffect;  // efekt bílého záblesku pøi focení
    public AudioSource shutterSound; // zvuk závìrky

    [Header("Settings")]
    public float moveSpeed = 8f;         // rychlost posunu modelu
    public float rotationSpeed = 8f;     // rychlost rotace modelu
    public float normalFOV = 60f;        // bìžný FOV
    public float zoomFOV = 35f;          // FOV pøi zamìøení
    public float fovSmoothSpeed = 6f;    // rychlost zmìny FOV

    private bool isAiming = false;

    void Start()
    {
        if (photoCamera != null)
            photoCamera.enabled = false;

        if (flashEffect != null)
            flashEffect.alpha = 0f;

        if (photoUI != null)
            photoUI.alpha = 0f;
    }

    void Update()
    {
        // --- AIMING (RMB) ---
        if (Input.GetMouseButtonDown(1)) isAiming = true;
        if (Input.GetMouseButtonUp(1)) isAiming = false;

        // --- CAMERA SWITCH ---
        if (photoCamera != null)
        {
            if (isAiming)
            {
                playerCamera.enabled = false;
            }
            else
            {
                playerCamera.enabled = isAiming;
            }
        }

        // --- MOVE CAMERA MODEL ---
        Transform target = isAiming ? aimPosition : restPosition;

        cameraModel.position = Vector3.Lerp(
            cameraModel.position,
            target.position,
            Time.deltaTime * moveSpeed
        );

        cameraModel.rotation = Quaternion.Lerp(
            cameraModel.rotation,
            target.rotation,
            Time.deltaTime * rotationSpeed
        );

        // --- FOV ANIMATION ---
        float targetFOV = isAiming ? zoomFOV : normalFOV;
        Camera activeCam = isAiming && photoCamera != null ? photoCamera : playerCamera;

        activeCam.fieldOfView = Mathf.Lerp(
            activeCam.fieldOfView,
            targetFOV,
            Time.deltaTime * fovSmoothSpeed
        );

        // --- UI ANIMATION ---
        if (photoUI != null)
        {
            float targetAlpha = isAiming ? 1f : 0f;
            photoUI.alpha = Mathf.Lerp(photoUI.alpha, targetAlpha, Time.deltaTime * 8f);
        }

        // --- TAKE PHOTO ---
        if (isAiming && Input.GetMouseButtonDown(0))
            TakePhoto();
    }
    void LateUpdate()
    {
        if (photoCamera != null)
        {
            photoCamera.transform.position = playerCamera.transform.position;
            photoCamera.transform.rotation = playerCamera.transform.rotation;
        }
    }

    void TakePhoto()
    {
        Debug.Log("Photo taken!");

        if (shutterSound != null)
            shutterSound.Play();

        if (flashEffect != null)
            StartCoroutine(FlashRoutine());
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        flashEffect.alpha = 1f;
        yield return new WaitForSeconds(0.05f);

        while (flashEffect.alpha > 0)
        {
            flashEffect.alpha -= Time.deltaTime * 3f;
            yield return null;
        }
    }
}