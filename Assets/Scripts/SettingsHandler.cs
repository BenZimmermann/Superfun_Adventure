using UnityEngine;
using UnityEngine.UI;
public class SettingsHandler : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;

    [Header("UI References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle muteAllToggle;
    //update the UI elements with current settings when the settings menu is opened
    public void OnBackPressed()
    {
        SettingsCanvas.SetActive(false);
        SettingsManager.Instance.SaveSettings();
    }
    public void OnResetPressed()
    {
        SettingsManager.Instance.ResetToDefault();
    }
    public void OnMasterVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMasterVolume(value);
    }
    public void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
    }
    public void OnSFXVolumeChanged(float value)
    {
        SettingsManager.Instance.SetSfxVolume(value);
    }
    public void OnMuteAll(bool isMuted)
    {
        SettingsManager.Instance.SetMuteAll(isMuted);
    }
    public void MouseSensitivity(float value)
    {
        SettingsManager.Instance.SetMouseSensitivity(value);
    }

}
