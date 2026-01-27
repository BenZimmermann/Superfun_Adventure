using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private GameObject GameOverCanvas;
    public void OnRestartPressed()
    {
        GameManager.Instance.ContinueGame();
        GameStateManager.Instance.SetState(GameState.Playing);
        GameOverCanvas.SetActive(false);
    }
    public void OnQuitPressed()
    {
        GameStateManager.Instance.SetState(GameState.MainMenu);
        GameOverCanvas.SetActive(false);
    }
}
