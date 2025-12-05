using TMPro;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public TextMeshProUGUI interactPrompt;

    private Camera cam;
    private BookInteract focusedBook;

    void Start()
    {
        cam = Camera.main;
        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        if (BookInteract.IsUIOpen || CameraSystem.IsAimingGlobal)
        {
            if (interactPrompt != null && interactPrompt.gameObject.activeSelf)
                interactPrompt.gameObject.SetActive(false);

            return;
        }


        if (focusedBook != null && focusedBook.IsReading)
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

                if (interactPrompt != null && !interactPrompt.gameObject.activeSelf)
                {
                    interactPrompt.text = "[E] Read";
                    interactPrompt.gameObject.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                    book.OpenBook();

                return;
            }

            Door door = hit.collider.GetComponentInParent<Door>();
            if (door != null)
            {
                if (door.isLocked)
                {
                    if (!interactPrompt.gameObject.activeSelf)
                    {
                        interactPrompt.text = "You need to read the book first";
                        interactPrompt.gameObject.SetActive(true);
                    }

                    return;
                }

                interactPrompt.text = door.isOpen ? "[E] Close" : "[E] Open";
                interactPrompt.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                    door.ToggleDoor();

                return;
            }

        }

        focusedBook = null;
        if (interactPrompt != null && interactPrompt.gameObject.activeSelf)
            interactPrompt.gameObject.SetActive(false);
    }
}
