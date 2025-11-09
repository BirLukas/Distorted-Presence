using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera playerCamera;
    public float zoomFOV = 30f;
    public float normalFOV = 60f;
    public float zoomSpeed = 5f;

    [Header("Efects")]
    public AudioSource shutterSound;
    public CanvasGroup flashEffect;

    private bool isAiming = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            isAiming = true;
        if (Input.GetMouseButtonUp(1))
            isAiming = false;

        float targetFOV = isAiming ? zoomFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

        if (isAiming && Input.GetMouseButtonDown(0))
        {
            TakePhoto();
        }
    }

    void TakePhoto()
    {
        Debug.Log("Photo Taken");

        if (shutterSound != null)
            shutterSound.Play();

        if (flashEffect != null)
            StartCoroutine(FlashRoutine());

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Debug.Log("In photo taken: " + hit.collider.name);
        }
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        flashEffect.alpha = 1f;
        yield return new WaitForSeconds(0.1f);
        while (flashEffect.alpha > 0)
        {
            flashEffect.alpha -= Time.deltaTime * 2f;
            yield return null;
        }
    }
}
