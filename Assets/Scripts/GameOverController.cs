using UnityEngine;
using Unity.UI;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private GameObject GameOverCanvas;

    private void Awake()
    {
        GameStateManager.Instance.OnStateChanged += HandleStateChanged;
        GameOverCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        Debug.LogWarning($"HandleStateChanged called: {oldState} -> {newState}");
        bool show = newState == GameState.GameOver;
        GameOverCanvas.SetActive(show);
    }
    public void OnRestartPressed()
    {
        GameManager.Instance.ContinueGame();
       
    }
    public void OnQuitPressed()
    {
        GameStateManager.Instance.SetState(GameState.MainMenu);
        
    }
}
