using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject PauseCanvas;
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private SettingsHandler settingsHandler;
    public void OnResumePressed()
    {
        //later MainMenu to Playing
        GameStateManager.Instance.LastGameState();
        PauseCanvas.SetActive(false);
    }
    public void QuitPressed()
    {
        GameStateManager.Instance.SetState(GameState.MainMenu);
        PauseCanvas.SetActive(false);
    }
    public void OnSettingsPressed()
    {
        settingsHandler.UpdateUIOnLoad();
        GameStateManager.Instance.SetState(GameState.Settings);
        SettingsCanvas.SetActive(true);
    }
}
