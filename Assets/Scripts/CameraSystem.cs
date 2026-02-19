using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class CameraSystem : MonoBehaviour
{
    [Header("Setup Objekty")]
    public GameObject cameraModel;
    public GameObject cameraUI;
    public CanvasGroup flashGroup;
    public Camera playerCamera;

    [Header("Nastavení Pohledu a Zoomu")]
    public float defaultFOV = 60f;
    public float aimedFOV = 30f;
    public float aimSpeed = 5f;

    [Header("Nastavení Blesku")]
    public float flashDuration = 0.1f;

    private PhotoCapture photoCapture;
    private bool isAimed = false;
    public static bool IsAimingGlobal { get; private set; }

    void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Chyba: Player Camera není pøiøazena!");
            return;
        }

        photoCapture = playerCamera.GetComponent<PhotoCapture>();

        if (cameraUI != null) cameraUI.SetActive(false);
        if (flashGroup != null) flashGroup.alpha = 0f;

        playerCamera.fieldOfView = defaultFOV;
    }
    public void OnAim(InputValue value)
    {
        isAimed = value.isPressed;

        if (isAimed) AimCamera();
        else StopAiming();
    }
    public void OnFire(InputValue value)
    {
        if (value.isPressed && isAimed)
        {
            TakePhoto();
        }
    }
    void Update()
    {
        float targetFOV = isAimed ? aimedFOV : defaultFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);

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
                StartCoroutine(FlashEffect());
            }
            photoCapture.TryCaptureAnomaly();
        }
    }
    IEnumerator FlashEffect()
    {
        if (flashGroup != null) flashGroup.alpha = 1f;
        yield return new WaitForSeconds(flashDuration);
        if (flashGroup != null) flashGroup.alpha = 0f;
    }
}