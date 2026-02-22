using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Nastaven� citlivosti")]
    public float mouseSensitivity = 0.1f;

    [Header("Reference z Hierarchie")]
    public Transform playerBody;
    public Transform cameraTransform;

    private float xRotation = 0f;
    private Vector2 lookInput;

    void Start()
    {
        UpdateCursorState();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            UpdateCursorState();
        }
    }

    public void UpdateCursorState()
    {
        if (!BookInteract.IsUIOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void LateUpdate()
    {
        if (SanityManager.Instance != null && SanityManager.Instance.IsGameOver) return;
        if (BookInteract.IsUIOpen) return;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);

        lookInput = Vector2.zero;
    }
}
