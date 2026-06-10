using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused = false;

    [Header("UI References")]
    [Tooltip("Přiřaď sem panel s Pause Menu")]
    public GameObject pauseMenuUI;
    
    [Tooltip("Přiřaď sem panel s Nastavením (pokud je oddělený)")]
    public GameObject settingsMenuUI;

    void Start()
    {
        // Ujistíme se, že při startu je hra normálně běžící
        ResumeGame();
    }

    void Update()
    {
        // Kontrola stisku klávesy Esc
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Zabráníme pauzování, pokud je otevřená kniha nebo jiný UI prvek
            if (BookInteract.IsUIOpen && !IsPaused) return;

            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // Zastaví čas ve hře
        
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        
        // Zobrazíme a odemkneme kurzor myši
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // Znovu spustí čas
        
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        
        // Pokud není otevřena kniha, kurzor opět skryjeme
        if (!BookInteract.IsUIOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    // Metoda volaná z tlačítka Settings
    public void OpenSettings()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(true);
    }
    
    // Metoda volaná z tlačítka Zpět v Settings
    public void CloseSettings()
    {
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    // Pro tlačítko Quit do hlavního menu
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
