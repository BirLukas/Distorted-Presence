using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 0.1f;

    [Header("References")]
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
        {
            // Base rotation from mouse
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Apply GlitchEffect if present
            GlitchEffect glitch = cameraTransform.GetComponent<GlitchEffect>();
            if (glitch != null)
            {
                cameraTransform.localRotation *= glitch.GetRotationOffset();
                // We assume original position is what it was at Start, 
                // but MouseLook doesn't normally move the camera position.
                // We'll apply the offset to the current position.
                // Note: This might need a 'base' position if other things move the camera.
                cameraTransform.localPosition = new Vector3(0, 0.6f, 0) + glitch.GetPositionOffset();
            }
        }

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);

        lookInput = Vector2.zero;
    }

}
