using UnityEngine;

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
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void LoadSettings()
    {
        Debug.Log("Settings Loaded");
    }
    public void SaveSettings()
    {
        Debug.Log("Settings Saved");
    }
    public void UpdateSetting()
    {
        Debug.Log("Setting Updated");
    }
}
