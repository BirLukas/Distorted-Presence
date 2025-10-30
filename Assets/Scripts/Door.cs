using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorVisual;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isMoving = false;

    [Header("Lock Settings")]
    public bool isLocked = false;

    void Start()
    {
        if (doorVisual == null)
            doorVisual = transform;

        closedRotation = doorVisual.localRotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        if (Quaternion.Angle(doorVisual.localRotation, targetRotation) > 0.1f)
        {
            doorVisual.localRotation = Quaternion.Slerp(doorVisual.localRotation, targetRotation, Time.deltaTime * openSpeed);
            isMoving = true;
        }
        else
        {
            doorVisual.localRotation = targetRotation;
            isMoving = false;
        }
    }

    public void ToggleDoor()
    {
        if (isLocked) return;
        if (isMoving) return;

        isOpen = !isOpen;
    }
    public void LockDoor()
    {
        isLocked = true;
    }

    public void UnlockDoor()
    {
        isLocked = false;
    }
}
