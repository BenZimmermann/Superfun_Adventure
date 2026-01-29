using UnityEngine;
using System.Collections;
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
    private int currentLevel = 0; //level from save data
    private bool isNewGame = true;

    public int CurrentLevel => currentLevel;
    public bool IsNewGame => isNewGame;

    [SerializeField] private GameObject playerPrefab;
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

        StartCoroutine(ApplySaveDataAfterDelay(0.1f));
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
        StartCoroutine(ApplySaveDataAfterDelay(0.1f));
    }
    /// <summary>
    /// Restart from last save point after death
    /// </summary>
    /// <returns></returns>
    private IEnumerator RestartFromLastSave()
    {
        yield return new WaitForSeconds(2f);

  
        currentSaveData = SaveManager.Instance.LoadGame();

        if (currentSaveData != null)
        {
            currentLevel = currentSaveData.currentLevel;
            GameStateManager.Instance.SetState(GameState.Playing);
            LevelManager.Instance.LoadLevel(currentLevel);
            StartCoroutine(ApplySaveDataAfterDelay(0.1f));
        }
        else
        {
            StartNewGame();
        }
    }

    /// <summary>
    /// Delay before applying save data 
    /// </summary>
    private IEnumerator ApplySaveDataAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplySaveData();
    }
    private void ApplySaveData()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("No save data to apply!");
            return;
        }
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not assigned in GameManager!");
            return;
        }
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            Destroy(existingPlayer);
        }

        // 2. Determine spawn position
        Vector3 spawnPosition = Vector3.zero; // Default spawn
        if (currentSaveData.lastSavePoint != Vector2.zero)
        {
            spawnPosition = currentSaveData.lastSavePoint;
        }

        // 3. Instantiate the Prefab
        GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        CameraController camController = Object.FindFirstObjectByType<CameraController>();
        //weitere Attribute wie Health, Psychometer etc. hier anwenden
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}  
