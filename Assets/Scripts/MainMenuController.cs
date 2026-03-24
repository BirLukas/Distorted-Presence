using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
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
        // TODO: Enable Settings UI panel when created
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