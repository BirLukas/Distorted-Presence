using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform cameraHolder;
    public Transform cameraModel;
    public Transform aimPosition;
    public Transform restPosition;
    public CanvasGroup photoUI;
    public CanvasGroup flashEffect;
    public AudioSource shutterSound;

    [Header("Camera Settings")]
    public float zoomFOV = 35f;
    public float normalFOV = 60f;
    public float transitionSpeed = 6f;
    public float cameraMoveSpeed = 8f;

    private bool isAiming = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            isAiming = true;
        if (Input.GetMouseButtonUp(1))
            isAiming = false;
     
        float targetFOV = isAiming ? zoomFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);

        Transform target = isAiming ? aimPosition : restPosition;
        cameraModel.position = Vector3.Lerp(cameraModel.position, target.position, Time.deltaTime * cameraMoveSpeed);
        cameraModel.rotation = Quaternion.Lerp(cameraModel.rotation, target.rotation, Time.deltaTime * cameraMoveSpeed);

        if (photoUI != null)
            photoUI.alpha = Mathf.Lerp(photoUI.alpha, isAiming ? 1f : 0f, Time.deltaTime * 8f);

        if (isAiming && Input.GetMouseButtonDown(0))
        {
            TakePhoto();
        }
    }

    void TakePhoto()
    {
        Debug.Log("Photo Taken!");
        if (shutterSound != null) shutterSound.Play();
        if (flashEffect != null) StartCoroutine(FlashRoutine());
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        flashEffect.alpha = 1f;
        yield return new WaitForSeconds(0.1f);
        while (flashEffect.alpha > 0)
        {
            flashEffect.alpha -= Time.deltaTime * 3f;
            yield return null;
        }
    }
}
