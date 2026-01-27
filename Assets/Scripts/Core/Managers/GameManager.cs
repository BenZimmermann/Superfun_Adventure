using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// -Starts new game
    /// -countinue game
    /// -quit game
    /// -after death restart from last save
    /// ----
    /// Intern:
    /// -current level
    /// -flag: new game/load
    /// </summary>
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    private int currentLevel = 0; //level from save data?
    private bool isNewGame = true;

    public int CurrentLevel => currentLevel;
    public bool IsNewGame => isNewGame;

    private SaveData currentSaveData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    /// <summary>
    /// starts a new game (resets all data)
    /// </summary>
    public void StartNewGame()
    {
        isNewGame = true;
        currentSaveData = SaveManager.Instance.CreateNewSave();
        currentLevel = currentSaveData.currentLevel;

        GameStateManager.Instance.SetState(GameState.Playing);
        LevelManager.Instance.LoadLevel(currentLevel);
    }
    public void ContinueGame()
    {
        if(!SaveManager.Instance.HasSave())
        {
            Debug.LogWarning("No Save File Found, Starting New Game Instead");
            StartNewGame();
            return;
        }
        isNewGame = false;
        currentSaveData = SaveManager.Instance.LoadGame();

        if(currentSaveData == null)
        {
            Debug.LogError("Failed to Load Save Data, Starting New Game Instead");
            StartNewGame();
            return;
        }
        currentLevel = currentSaveData.currentLevel;
        Debug.Log($"Game loaded, Level: {currentSaveData.level}");
        GameStateManager.Instance.SetState(GameState.Playing);
        LevelManager.Instance.LoadLevel(currentLevel);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void RestartFromLastSave()
    {
        ContinueGame();
    }
}  
