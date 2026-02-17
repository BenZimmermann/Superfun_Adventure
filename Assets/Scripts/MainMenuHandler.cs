using Unity.VisualScripting;
using UnityEngine;
using Unity.UI;
using static CorruptionManager;
public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private SettingsHandler settingsHandler;
    private CorruptionManager manager;

    private void OnEnable()
    {
        // Falls SaveData bereits geladen wurde bevor wir subscriben
        if (GameManager.Instance.currentSaveData != null)
        {
            OnSaveDataLoaded();
            return;
        }

        // Ansonsten auf das Event warten
        GameManager.Instance.OnSaveDataLoaded += OnSaveDataLoaded;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSaveDataLoaded -= OnSaveDataLoaded;
    }

    private void OnSaveDataLoaded()
    {
        CorruptionManager.Instance.SetCorruptionState();

        if (CorruptionManager.Instance.currentCorruptionState == CorruptionState.Low)
        {
            Debug.LogWarning("Corruption State: Low");
        }
    }
    public void OnContinuePressed()
    {
        SaveData saveData = SaveManager.Instance.LoadGame();

        if (saveData != null)
        {
            GameManager.Instance.ContinueGame();
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
        GameManager.Instance.StartNewGame();

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
