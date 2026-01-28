using Unity.VisualScripting;
using UnityEngine;

public class PauseTester : MonoBehaviour
{
    [SerializeField] private GameObject PauseCanvas;
    [SerializeField] private GameObject GameOverCanvas;
    void Update()
    {
        CPressed();
        XPressed();
        Debug.LogWarning(GameStateManager.Instance.CurrentState);

    }
    private void CPressed()

    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Y gedrückt");
            if (GameStateManager.Instance.CurrentState == GameState.GameOver)
            {
                GameOverCanvas.SetActive(false);
                Debug.Log("GameOverCanvas deaktiviert");
                GameStateManager.Instance.SetState(GameState.Playing);
            }
            else if (GameStateManager.Instance.CurrentState == GameState.Playing)
            {
                GameOverCanvas.SetActive(true);
                Debug.Log("GameOverCanvas aktiviert");
                GameStateManager.Instance.SetState(GameState.GameOver);
            }
            Debug.Log("Y gedrückt und ausgeführt");
        }

    }
    private void XPressed()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("X gedrückt");
            if (GameStateManager.Instance.CurrentState == GameState.Paused)
            {
                PauseCanvas.SetActive(false);
                Debug.Log("PauseCanvas deaktiviert");
                GameStateManager.Instance.SetState(GameState.Playing);
            }
            else if (GameStateManager.Instance.CurrentState == GameState.Playing)
            {
                PauseCanvas.SetActive(true);
                Debug.Log("PauseCanvas aktiviert");
                GameStateManager.Instance.SetState(GameState.Paused);
            }
            Debug.Log("X gedrückt und ausgeführt");
        }
    }
}
