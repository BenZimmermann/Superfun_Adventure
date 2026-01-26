using UnityEngine;

public class SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool muteAll;
    public float mouseSensitivity;
    //keybindings can be added as needed

    public SettingsData()
        {
            masterVolume = 1.0f;
            musicVolume = 1.0f;
            sfxVolume = 1.0f;
            muteAll = false;
            mouseSensitivity = 1.0f;
        }
}

