using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject PauseCanvas;
    [SerializeField] private GameObject SettingsCanvas;
    public void OnResumePressed()
    {
        //later MainMenu to Playing
        GameStateManager.Instance.SetState(GameState.MainMenu);
        PauseCanvas.SetActive(false);
    }
    public void QuitPressed()
    {
        GameStateManager.Instance.SetState(GameState.MainMenu);
        PauseCanvas.SetActive(false);
    }
    public void OnSettingsPressed()
    {
        GameStateManager.Instance.SetState(GameState.Settings);
        SettingsCanvas.SetActive(true);
    }
}
