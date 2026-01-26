using UnityEngine;
using System.IO;
public class SaveManager : MonoBehaviour
{
    /// <summary>
    /// Data:
    /// -Level
    /// -health
    /// -Psychometer
    /// -last save Point
    /// -Distortion Level
    /// -Current level
    /// ----
    /// -write save
    /// -read save
    /// -check if save exists
    /// </summary>
    public static SaveManager Instance { get; private set; }
    private string savePath;
    private const string SAVE_FILE_NAME = "savegame.json";
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log($"Save Path: {savePath} ");
    }
    /// <summary>
    /// saves the game data to a json file
    /// </summary>
    /// <param name="data"></param>
    public void SaveGame(SaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Game Saved");
        }
        catch
        { 
            Debug.LogError("Failed to Save Game");
        }
    }
    /// <summary>
    /// loads the game data from a json file
    /// </summary>
    /// <returns></returns>
    public SaveData LoadGame()
    {
        if(!HasSave())
        {
            Debug.LogWarning("No Save File Found");
            return new SaveData();
        }
        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Game Loaded");
            return data;
        }
        catch(System.Exception e) 
        {
            Debug.LogError($"Failed to Load Game: {e.Message}");
            return new SaveData();
        }
    }
    public SaveData CreateNewSave()
    {
        SaveData newData = new SaveData();
        SaveGame(newData);
        return newData;
    }
    /// <summary>
    /// checks if a save file exists
    /// </summary>
    /// <returns></returns>
    public bool HasSave()
    {
        bool exists = File.Exists(savePath);
        Debug.Log("Save File Exists: " + exists);
        return exists;
    }

}
