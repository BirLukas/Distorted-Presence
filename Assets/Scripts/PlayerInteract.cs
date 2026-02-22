using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public TextMeshProUGUI interactPrompt;

    private Camera cam;
    private BookInteract focusedBook;
    private Door focusedDoor;

    [Header("Hold to Interact Settings")]
    public float requiredHoldTime = 2f;
    private bool isHoldingInteract = false;
    private float currentHoldTime = 0f;

    void Start()
    {
        cam = Camera.main;
        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(false);
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            if (focusedBook != null)
            {
                focusedBook.OpenBook();
            }
            else if (focusedDoor != null && !focusedDoor.isLocked && !focusedDoor.canEndDay)
            {
                focusedDoor.ToggleDoor();
            }
        }
    }


    void Update()
    {
        // Poll input directly for hold mechanics since OnInteract is only called on state change
        isHoldingInteract = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        
        if (!isHoldingInteract)
        {
            currentHoldTime = 0f;
        }

        focusedBook = null;
        focusedDoor = null;

        if (BookInteract.IsUIOpen || CameraSystem.IsAimingGlobal || (SanityManager.Instance != null && SanityManager.Instance.IsGameOver))
        {
            if (interactPrompt != null && interactPrompt.gameObject.activeSelf)
                interactPrompt.gameObject.SetActive(false);
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            BookInteract book = hit.collider.GetComponent<BookInteract>();
            if (book != null)
            {
                focusedBook = book;
                if (interactPrompt != null)
                {
                    interactPrompt.text = "[E] Read";
                    interactPrompt.gameObject.SetActive(true);
                }
                return;
            }

            Door door = hit.collider.GetComponentInParent<Door>();
            if (door != null)
            {
                focusedDoor = door;
                if (interactPrompt != null)
                {
                    if (door.canEndDay)
                    {
                        if (isHoldingInteract)
                        {
                            currentHoldTime += Time.deltaTime;
                            if (currentHoldTime >= requiredHoldTime)
                            {
                                currentHoldTime = 0f;
                                door.canEndDay = false; // Prevent repeated triggers
                                FindFirstObjectByType<DayManager>().EndDay();
                                interactPrompt.gameObject.SetActive(false);
                                return;
                            }
                            int progress = Mathf.RoundToInt((currentHoldTime / requiredHoldTime) * 100);
                            interactPrompt.text = $"Ending Day... {progress}%";
                        }
                        else
                        {
                            currentHoldTime = 0f;
                            interactPrompt.text = "[Hold E] End Day";
                        }
                    }
                    else if (door.isLocked)
                    {
                        interactPrompt.text = "You need to read the book first";
                    }
                    else
                    {
                        interactPrompt.text = door.isOpen ? "[E] Close" : "[E] Open";
                    }
                    interactPrompt.gameObject.SetActive(true);
                }
                return;
            }
        }
        
        // Reset hold time if not looking at a door that can end the day
        currentHoldTime = 0f;

        if (interactPrompt != null && interactPrompt.gameObject.activeSelf)
            interactPrompt.gameObject.SetActive(false);
    }
}