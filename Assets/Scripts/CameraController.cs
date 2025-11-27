using UnityEngine;

public class CameraController : MonoBehaviour
{
    private MeshRenderer[] cameraRenderers;
    public static bool IsAimingGlobal = false;


    [Header("Reference")]
    public Camera playerCamera;
    public Camera photoCamera;
    public Transform cameraModel;
    public Transform restPosition;
    public Transform aimPosition;
    public CanvasGroup photoUI;
    public CanvasGroup flashEffect;
    public AudioSource shutterSound;

    [Header("Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 8f;
    public float normalFOV = 60f;
    public float zoomFOV = 35f;
    public float fovSmoothSpeed = 6f;

    private bool isAiming = false;

    void Start()
    {
        cameraRenderers = cameraModel.GetComponentsInChildren<MeshRenderer>(true);

        if (photoCamera != null)
            photoCamera.enabled = false;

        if (flashEffect != null)
            flashEffect.alpha = 0f;

        if (photoUI != null)
            photoUI.alpha = 0f;
    }

    void Update()
    {
        IsAimingGlobal = isAiming;

        if (BookInteract.IsUIOpen)
        {
            isAiming = false;
            return;
        }

        // AIMING (Right Mouse Button)
        if (Input.GetMouseButtonDown(1)) isAiming = true;
        if (Input.GetMouseButtonUp(1)) isAiming = false;

        // CAMERA SWITCH (Correct for Unity 6 URP)
        if (photoCamera != null)
        {
            playerCamera.enabled = true;
            photoCamera.enabled = isAiming;

            foreach (var r in cameraRenderers)
            {
                r.enabled = !isAiming;
            }
        }



        // MOVE CAMERA MODEL
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

        // FOV ANIMATION
        float targetFOV = isAiming ? zoomFOV : normalFOV;

        // hlavní pohled musí mít vždy FOV, aby fungoval MouseLook
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * fovSmoothSpeed
        );

        // PhotoCamera musí mít stejný zoom, jinak AIM nic neudìlá
        if (photoCamera != null)
        {
            photoCamera.fieldOfView = Mathf.Lerp(
                photoCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * fovSmoothSpeed
            );
        }

        // UI ANIMATION
        if (photoUI != null)
        {
            float targetAlpha = isAiming ? 1f : 0f;
            photoUI.alpha = Mathf.Lerp(photoUI.alpha, targetAlpha, Time.deltaTime * 8f);
        }

        // TAKE PHOTO
        if (isAiming && Input.GetMouseButtonDown(0))
            TakePhoto();
    }

    void LateUpdate()
    {
        // PHOTO CAMERA FOLLOW MAIN CAMERA
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