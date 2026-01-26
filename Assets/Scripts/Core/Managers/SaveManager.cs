using UnityEngine;

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
    //public void SaveGame(SaveData data)
    //{
    //    Debug.Log("Game Saved");
    //}
    public void LoadGame()
    {
        Debug.Log("Game Loaded");
    }
    public void HasSave()
    {
        Debug.Log("Checked for Saved Game");
    }
}
