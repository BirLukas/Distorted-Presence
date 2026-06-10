using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainButtonsPanel;
    public GameObject settingsMenuPanel;

    void Start()
    {
        Button playButton = GameObject.Find("StartGameButton")?.GetComponent<Button>();
        Button settingsButton = GameObject.Find("SettingsButton")?.GetComponent<Button>();
        Button quitButton = GameObject.Find("QuitGameButton")?.GetComponent<Button>();

        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    public void PlayGame()
    {
        Debug.Log("Starting game: loading MainScene");
        SceneManager.LoadScene("MainScene");
    }

    public void OpenSettings()
    {
        Debug.Log("Settings button clicked!");
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }
}