using Unity.VisualScripting;
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private SettingsHandler settingsHandler;

    public void OnContinuePressed()
    {
        SaveData saveData = SaveManager.Instance.LoadGame();

        if (saveData != null)
        {
            LevelManager.Instance.LoadLevel(saveData.currentLevel);
            GameStateManager.Instance.SetState(GameState.Playing);
        }
        else
        {
            Debug.LogWarning("Kein gültiger Spielstand gefunden!");
        }
    }
        public void OnStartPressed()
    {
        SaveManager.Instance.CreateNewSave();
        LevelManager.Instance.LoadLevel(0);
        GameStateManager.Instance.SetState(GameState.Playing);
    }
    public void OnSettingPressed()
    {
        settingsHandler.UpdateUIOnLoad();
        SettingsCanvas.SetActive(true);
        GameStateManager.Instance.SetState(GameState.Settings);
    }
    public void OnQuitPressed()
    {
        GameManager.Instance.QuitGame();
    }
}
