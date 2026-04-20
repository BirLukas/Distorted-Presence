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

    private Collider doorCollider;
    private CharacterController playerController;

    [Header("End Day Settings")]
    public bool canEndDayAfterUnlock = false;
    public float endDayDelay = 120f;
    [HideInInspector] public bool canEndDay = false;
    private float unlockTimer = 0f;

    [Header("Lock Settings")]
    public bool isLocked = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    void Start()
    {
        if (doorVisual == null)
            doorVisual = transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        closedRotation = doorVisual.localRotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;

        doorCollider = doorVisual.GetComponentInChildren<Collider>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<CharacterController>();
        }

        // Fallback if tag is missing or incorrect
        if (playerController == null)
        {
            playerController = Object.FindFirstObjectByType<CharacterController>();
        }
    }

    void Update()
    {
        if (canEndDayAfterUnlock && !isLocked && !canEndDay)
        {
            unlockTimer += Time.deltaTime;
            if (unlockTimer >= endDayDelay)
            {
                canEndDay = true;
            }
        }

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        if (Quaternion.Angle(doorVisual.localRotation, targetRotation) > 0.1f)
        {
            Quaternion nextRotation = Quaternion.Slerp(doorVisual.localRotation, targetRotation, Time.deltaTime * openSpeed);
            bool canMove = true;

            if (doorCollider != null && playerController != null)
            {
                // Predictive check: see if the NEXT rotation would hit the player's CURRENT position.
                // We do this BEFORE moving the door to prevent Unity's automatic pushing behavior.
                Vector3 direction;
                float distance;
                bool wouldOverlap = Physics.ComputePenetration(
                    doorCollider, doorCollider.transform.position, nextRotation,
                    playerController, playerController.transform.position, playerController.transform.rotation,
                    out direction, out distance
                );

                if (wouldOverlap && distance > 0.01f)
                {
                    canMove = false;
                }
            }

            if (canMove)
            {
                doorVisual.localRotation = nextRotation;
                isMoving = true;
            }
            else
            {
                // Door is blocked, stay still but keep isMoving true so we keep checking
                isMoving = true;
            }
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

        if (audioSource != null)
        {
            if (isOpen && openSound != null)
                audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null)
                audioSource.PlayOneShot(closeSound);
        }
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
