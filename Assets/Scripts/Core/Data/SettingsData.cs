using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool muteAll;
    public float mouseSensitivity;
    //keybindings can be added as needed
    public List<string> bindingKeys;
    public List<string> bindingValues;


    public SettingsData()
        {
            masterVolume = 1.0f;
            musicVolume = 1.0f;
            sfxVolume = 1.0f;
            muteAll = false;
            mouseSensitivity = 1.0f;

        bindingKeys = new List<string>();
        bindingValues = new List<string>();

    }
    public void SetKeybinding(string actionName, string bindingPath)
    {
        int index = bindingKeys.IndexOf(actionName);

        if (index >= 0)
        {
            bindingValues[index] = bindingPath;
        }
        else
        {
            bindingKeys.Add(actionName);
            bindingValues.Add(bindingPath);
        }
    }

    public string GetKeybinding(string actionName)
    {
        int index = bindingKeys.IndexOf(actionName);
        return (index >= 0 && index < bindingValues.Count) ? bindingValues[index] : null;
    }

    public void RemoveKeybinding(string actionName)
    {
        int index = bindingKeys.IndexOf(actionName);

        if (index >= 0)
        {
            bindingKeys.RemoveAt(index);
            bindingValues.RemoveAt(index);
        }
    }

    public Dictionary<string, string> GetAllKeybindings()
    {
        Dictionary<string, string> bindings = new Dictionary<string, string>();

        for (int i = 0; i < bindingKeys.Count && i < bindingValues.Count; i++)
        {
            bindings[bindingKeys[i]] = bindingValues[i];
        }

        return bindings;
    }

    public void ClearAllKeybindings()
    {
        bindingKeys.Clear();
        bindingValues.Clear();
    }

}

