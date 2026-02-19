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
            else if (focusedDoor != null && !focusedDoor.isLocked)
            {
                focusedDoor.ToggleDoor();
            }
        }
    }

    void Update()
    {
        focusedBook = null;
        focusedDoor = null;

        if (BookInteract.IsUIOpen || CameraSystem.IsAimingGlobal)
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
                    if (door.isLocked)
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
        if (interactPrompt != null && interactPrompt.gameObject.activeSelf)
            interactPrompt.gameObject.SetActive(false);
    }
}