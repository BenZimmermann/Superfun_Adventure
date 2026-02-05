using UnityEngine;
using Unity.UI;
public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject PauseCanvas;
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private SettingsHandler settingsHandler;

    private void Update()
    {
        if (InputController.EscIsUsed)
        {
            TogglePause();
        }
    }
    private void TogglePause()
    {

        if (GameStateManager.Instance.CurrentState == GameState.Paused)
        {
            PauseCanvas.SetActive(false);
            GameStateManager.Instance.SetState(GameState.Playing);
            GameManager.Instance.ResumeGame();
        }
        else if (GameStateManager.Instance.CurrentState == GameState.Playing)
        {
            PauseCanvas.SetActive(true);
            GameStateManager.Instance.SetState(GameState.Paused);
            GameManager.Instance.PauseGame();
           
        }
    }
    public void OnResumePressed()
    {
        GameStateManager.Instance.SetState(GameState.Playing);
        GameManager.Instance.ResumeGame();
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
