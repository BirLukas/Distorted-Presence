using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI Reference")]
    public TextMeshProUGUI tutorialText;

    private int currentDay;
    public bool IsTutorialActive => isTutorialActive;
    private bool isTutorialActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (GameProgressionManager.Instance != null)
        {
            currentDay = GameProgressionManager.Instance.currentDay;
        }
        else
        {
            currentDay = 1; // Default fallback
        }

        if (tutorialText != null)
        {
            tutorialText.text = "";
            tutorialText.gameObject.SetActive(false);
        }

        if (currentDay == 1)
        {
            BookInteract.TutorialCanOpen = false; // Lock the book initially
            StartCoroutine(Day1TutorialRoutine());
        }
        else if (currentDay == 2)
        {
            BookInteract.TutorialCanOpen = true; // Book is always openable after day 1
            StartCoroutine(Day2TutorialRoutine());
        }
        else
        {
            BookInteract.TutorialCanOpen = true;
            if (tutorialText != null) tutorialText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isTutorialActive && currentDay == 1)
        {
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                SkipTutorial();
            }
        }
    }

    public void SkipTutorial()
    {
        StopAllCoroutines();
        isTutorialActive = false;
        if (tutorialText != null) tutorialText.gameObject.SetActive(false);

        BookInteract.TutorialCanOpen = true;

        BookInteract book = FindFirstObjectByType<BookInteract>();
        if (book != null)
        {
            book.ForceRead();
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.ForceAllRoomsVisited();
        }
    }

    private IEnumerator Day1TutorialRoutine()
    {
        isTutorialActive = true;
        if (tutorialText != null) tutorialText.gameObject.SetActive(true);

        // 1. Movement
        SetText("Use WASD to move.\nHold SHIFT to sprint.\nJump with Space bar.\n\n[Press T to skip tutorial]");
        yield return new WaitForSeconds(15f);

        // 2. Camera & Film Warning
        SetText("Hold Right Mouse Button to aim the camera.\nLeft Click to take a photo.\nDo not waste film, you have limited shots!\n\n[Press T to skip tutorial]");
        yield return new WaitForSeconds(15f);

        // 3. Goal & Sanity
        SetText("Your goal is to find and photograph anomalies.\nYour Sanity drops if you miss them or photograph normal objects.\nThe day ends at 6:00 AM or you can leave earlier through the door in front.\n\n[Press T to skip tutorial]");
        yield return new WaitForSeconds(15f);

        // 4. Read Book
        SetText("Open the journal on the table to continue.\n\n[Press T to skip tutorial]");
        BookInteract.TutorialCanOpen = true; // Unlock the book

        // Wait until the player opens the book
        while (!BookInteract.IsUIOpen)
        {
            yield return null;
        }

        // Wait until they close the book
        while (BookInteract.IsUIOpen)
        {
            yield return null;
        }

        // 5. Walk through rooms
        while (RoomManager.Instance != null && !RoomManager.Instance.HaveAllRoomsBeenVisited())
        {
            int visited = RoomManager.Instance.VisitedRoomCount;
            int total = RoomManager.Instance.TotalRoomCount;
            SetText($"Walk through the rooms to begin the night.\nRooms checked: {visited} / {total}\n\n[Press T to skip tutorial]");
            yield return null;
        }

        // Finished Day 1 tutorial
        if (tutorialText != null) tutorialText.gameObject.SetActive(false);
        isTutorialActive = false;
    }

    private IEnumerator Day2TutorialRoutine()
    {
        isTutorialActive = true;
        if (tutorialText != null) tutorialText.gameObject.SetActive(true);

        SetText("Every night gets harder...");
        yield return new WaitForSeconds(5f);

        if (tutorialText != null) tutorialText.gameObject.SetActive(false);
        isTutorialActive = false;
    }

    private void SetText(string text)
    {
        if (tutorialText != null)
        {
            tutorialText.text = text;
        }
    }

    public void StopTutorial()
    {
        StopAllCoroutines();
        isTutorialActive = false;
        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
        }
    }
}
