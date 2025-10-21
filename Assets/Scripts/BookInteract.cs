using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
    private bool playerInRange = false;
    private bool isReading = false;
    private PlayerMovement characterController;

    void Start()
    {
        readPanel.SetActive(false);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (closeButton != null) closeButton.onClick.AddListener(CloseBook);

        characterController = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if (playerInRange && !isReading && Input.GetKeyDown(KeyCode.E))
        {
            OpenBook();
        }
    }

    void OpenBook()
    {
        isReading = true;
        readPanel.SetActive(true);
        ShowPage(0);
        LockPlayer(true);
    }

    void CloseBook()
    {
        isReading = false;
        readPanel.SetActive(false);
        LockPlayer(false);
    }

    void ShowPage(int index)
    {
        currentPage = Mathf.Clamp(index, 0, pages.Length - 1);
        bookText.text = pages[currentPage];

        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(currentPage < pages.Length - 1);
        closeButton.gameObject.SetActive(currentPage == pages.Length - 1);
    }

    void NextPage() => ShowPage(currentPage + 1);
    void PrevPage() => ShowPage(currentPage - 1);

    void LockPlayer(bool locked)
    {
        if (characterController != null)
        {
            characterController.enabled = !locked;
        }

        var look = Camera.main.GetComponent<MonoBehaviour>();
        if (look != null && look.GetType().Name.Contains("Look"))
        {
            look.enabled = !locked;
        }

        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered");
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
