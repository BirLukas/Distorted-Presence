using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class BookInteract : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject readPanel;
    public TextMeshProUGUI bookText;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    [Header("Book Content")]
    [TextArea(3, 10)]
    public string[] pages;

    private int currentPage = 0;
    private bool isReading = false;
    public bool IsReading => isReading;

    private bool hasBeenRead = false;
    public bool HasBeenReadOnce => hasBeenRead;

    private MouseLook mouseLook;
    public static bool IsUIOpen = false;

    void Start()
    {
        readPanel.SetActive(false);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (closeButton != null) closeButton.onClick.AddListener(CloseBook);

        mouseLook = FindFirstObjectByType<MouseLook>();
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && isReading)
        {
            CloseBook();
        }
    }

    public void OpenBook()
    {
        if (isReading) return;

        IsUIOpen = true;
        isReading = true;
        readPanel.SetActive(true);
        ShowPage(0);
        LockPlayer(true);

        if (!hasBeenRead)
            hasBeenRead = true;
    }

    public void CloseBook()
    {
        IsUIOpen = false;
        isReading = false;
        readPanel.SetActive(false);
        LockPlayer(false);
    }

    private void ShowPage(int index)
    {
        currentPage = Mathf.Clamp(index, 0, pages.Length - 1);
        bookText.text = pages[currentPage];

        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(currentPage < pages.Length - 1);
        closeButton.gameObject.SetActive(currentPage == pages.Length - 1);
    }

    private void NextPage() => ShowPage(currentPage + 1);
    private void PrevPage() => ShowPage(currentPage - 1);

    private void LockPlayer(bool locked)
    {
        var movement = FindFirstObjectByType<PlayerMovement>();

        if (movement != null)
            movement.enabled = !locked;

        if (mouseLook != null)
            mouseLook.enabled = !locked;

        if (locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}