using UnityEngine;

public class PauseTester : MonoBehaviour
{
    [SerializeField] private GameObject PauseCanvas;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
            {
            Debug.Log("X gedrückt");
            if (GameStateManager.Instance.CurrentState == GameState.Paused)
            {
                PauseCanvas.SetActive(false);
                Debug.Log("PauseCanvas deaktiviert");
                GameStateManager.Instance.SetState(GameState.MainMenu);
            }
            else if (GameStateManager.Instance.CurrentState == GameState.MainMenu)
            {
                PauseCanvas.SetActive(true);
                Debug.Log("PauseCanvas aktiviert");
                GameStateManager.Instance.SetState(GameState.Paused);
            }
            Debug.Log("X gedrückt und ausgeführt");
        }
        Debug.Log(GameStateManager.Instance.CurrentState);
    }
}
