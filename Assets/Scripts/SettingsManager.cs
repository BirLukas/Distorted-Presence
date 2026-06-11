using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements (Optional if used via events)")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown graphicsDropdown;
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    private Resolution[] resolutions;

    void Start()
    {
        // Setup resolutions
        resolutions = Screen.resolutions;
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            
            // Load saved resolution or use current
            if (PlayerPrefs.HasKey("ResolutionIndex"))
            {
                currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
                SetResolution(currentResolutionIndex);
            }
            
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // Setup graphics
        if (graphicsDropdown != null)
        {
            if (PlayerPrefs.HasKey("GraphicsQuality"))
            {
                int qualityIndex = PlayerPrefs.GetInt("GraphicsQuality");
                graphicsDropdown.value = qualityIndex;
                SetQuality(qualityIndex);
            }
            else
            {
                graphicsDropdown.value = QualitySettings.GetQualityLevel();
            }
            graphicsDropdown.RefreshShownValue();
        }

        // Setup volume
        if (volumeSlider != null)
        {
            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                float vol = PlayerPrefs.GetFloat("MasterVolume");
                volumeSlider.value = vol;
                SetVolume(vol);
            }
            else
            {
                volumeSlider.value = AudioListener.volume;
            }
        }

        // Setup sensitivity
        if (sensitivitySlider != null)
        {
            if (PlayerPrefs.HasKey("MouseSensitivity"))
            {
                float sens = PlayerPrefs.GetFloat("MouseSensitivity");
                sensitivitySlider.value = sens;
                SetSensitivity(sens);
            }
            else
            {
                sensitivitySlider.value = 0.1f;
            }
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
        
        MouseLook[] mouseLooks = FindObjectsByType<MouseLook>(FindObjectsSortMode.None);
        foreach (var ml in mouseLooks)
        {
            ml.mouseSensitivity = sensitivity;
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        PlayerPrefs.Save();
    }
}
