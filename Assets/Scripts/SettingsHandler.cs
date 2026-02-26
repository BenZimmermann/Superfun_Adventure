using UnityEngine;
using UnityEngine.UI;
//using static UnityEditorInternal.VersionControl.ListControl;
public class SettingsHandler : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;

    [Header("UI References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle muteAllToggle;
    public void UpdateUIOnLoad()
    {
        masterVolumeSlider.value = SettingsManager.Instance.GetMasterVolume();
        musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();
        sfxVolumeSlider.value = SettingsManager.Instance.GetSfxVolume();
        mouseSensitivitySlider.value = SettingsManager.Instance.GetMouseSensitivity();
        muteAllToggle.isOn = SettingsManager.Instance.GetMuteAll();

    }
    public void OnBackPressed()
    {
        SettingsCanvas.SetActive(false);
        SettingsManager.Instance.SaveSettings();
        GameStateManager.Instance.LastGameState();
    }
    public void OnResetPressed()
    {
        SettingsManager.Instance.ResetToDefault();
        UpdateUIOnLoad();
    }
    public void OnMasterVolumeChanged(Slider caller)
    {
        SettingsManager.Instance.SetMasterVolume(caller.value);
    }
    public void OnMusicVolumeChanged(Slider caller)
    {
        SettingsManager.Instance.SetMusicVolume(caller.value);
    }
    public void OnSFXVolumeChanged(Slider caller)
    {
        SettingsManager.Instance.SetSfxVolume(caller.value);
    }
    public void OnMuteAll(Toggle caller)
    {
        SettingsManager.Instance.SetMuteAll(caller);
    }
    public void MouseSensitivity(Slider caller)
    {
        SettingsManager.Instance.SetMouseSensitivity(caller.value);
    }

}
