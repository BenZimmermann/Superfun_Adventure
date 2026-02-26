using UnityEngine;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private GameObject GameOverCanvas;
    private bool gameOver = false;
    private float timer = 0f;

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
    private void Update()
    {
        //wenn game over und das level nicht 1 ist
        if (gameOver && GameManager.Instance.currentSaveData.currentLevel >= 1)
        {
            timer += Time.deltaTime;
            if (timer >= 3f) // Warte 1 Sekunde vor dem Starten der Animation
            {
                StartGameOverAnimation();
                timer = 0f;
            }
        }
        else if (gameOver && GameManager.Instance.currentSaveData.currentLevel == 0)
        {
            ShowCanvas();
            gameOver = false; // NEU — sofort stoppen
        }

    }
    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        Debug.LogWarning($"HandleStateChanged called: {oldState} -> {newState}");
        //bool show = newState == GameState.GameOver;
        if(newState == GameState.GameOver) 
        gameOver = true;

    }
    private void StartGameOverAnimation()
    {
        // Hier können Sie den Code für die Game-Over-Animation hinzufügen
        // Zum Beispiel könnten Sie eine Animation abspielen oder Effekte anzeigen
        ShowCanvas();
        Debug.Log("Game Over Animation gestartet");
        gameOver = false; // Setzen Sie dies auf false, wenn die Animation abgeschlossen ist
    }
    private void ShowCanvas()
    {
        if (!GameOverCanvas.activeSelf) // Schutz gegen mehrfachen Aufruf
        {
            GameOverCanvas.SetActive(true);
            Time.timeScale = 0;
            gameOver = false; // NEU — nicht mehr jeden Frame aufrufen
        }
    }
    public void OnRestartPressed()
    {
        //GameOverCanvas.SetActive(false);
        //Time.timeScale = 1; // Setze TimeScale auf 1, bevor du fortfährsts
        Debug.Log($"[GameOver] TimeScale vor Resume: {Time.timeScale}");
        //GameManager.Instance.ResumeGame(); // timeScale = 1 ZUERST, bevor ContinueGame Coroutinen startet
        Debug.Log($"[GameOver] TimeScale nach Resume: {Time.timeScale}");
        GameStateManager.Instance.SetState(GameState.Playing);
        GameManager.Instance.ContinueGame();
    }
    public void OnQuitPressed()
    {
        GameStateManager.Instance.SetState(GameState.MainMenu);
        
    }
}
