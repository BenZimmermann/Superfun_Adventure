using UnityEngine;
using System.IO;
using System.Collections.Generic;
public class SettingsManager : MonoBehaviour
{
    /// <summary>
    /// -Data:
    /// -volume levels
    /// -keybindings
    /// -(graphics settings)
    /// ---
    /// -load settings
    /// -save settings
    /// -update settings
    /// </summary>
    public static SettingsManager Instance { get; private set; }

    private SettingsData currentSettings;
    private string settingsPath;
    private const string SETTINGS_FILE_NAME = "settings.json";

    public System.Action OnSettingsChanged;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        settingsPath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);
        Debug.Log($"Settings Path: {settingsPath} ");
        LoadSettings();
    }
    public void LoadSettings()
    {
        if (!File.Exists(settingsPath))
        { 
            currentSettings = new SettingsData();
            SaveSettings();
            ApplySettings();
            return;
        }
        try
        {
            string json = File.ReadAllText(settingsPath);
            currentSettings = JsonUtility.FromJson<SettingsData>(json);

           ApplySettings();
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Failed to Load Settings: {e.Message}");
            currentSettings = new SettingsData();
            SaveSettings();
        }
    }
    #region Settings
    // ===== AUDIO SETTINGS =====
    public void SetMasterVolume(float volume)
    {
        currentSettings.masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = currentSettings.muteAll ? 0f : currentSettings.masterVolume;
        Debug.Log($"Master Volume set to: {currentSettings.masterVolume}");
    }

    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        Debug.Log($"Music Volume set to: {currentSettings.musicVolume}");
    }

    public void SetSfxVolume(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        Debug.Log($"SFX Volume set to: {currentSettings.sfxVolume}");
    }

    public void SetMuteAll(bool mute)
    {
        currentSettings.muteAll = mute;
        AudioListener.volume = mute ? 0f : currentSettings.masterVolume;
        Debug.Log($"Mute All set to: {mute}");
    }
    // ===== GAMEPLAY SETTINGS =====

    public void SetMouseSensitivity(float sensitivity)
    {
        currentSettings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
        Debug.Log($"Mouse Sensitivity set to: {currentSettings.mouseSensitivity}");
    }
    // ==== INPUT SETTINGS =====
    #endregion
    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(settingsPath, json);
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Failed to Save Settings: {e.Message}");
        }
    }
    public void ApplySettings()
    { 
       OnSettingsChanged?.Invoke();
    }
    public void UpdateSetting(SettingsData newSettings)
    {
        currentSettings = newSettings;
        ApplySettings();
        SaveSettings();
    }
}
