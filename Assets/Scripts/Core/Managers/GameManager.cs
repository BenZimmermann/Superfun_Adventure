using UnityEngine;
using System.Collections;
using System;
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

    public event Action<PlayerMovement> OnPlayerReady;

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
    public void RegisterPlayer(PlayerMovement player)
    {
        OnPlayerReady?.Invoke(player);
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

        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
            Destroy(existingPlayer);

        Vector3 spawnPosition = currentSaveData.lastSavePoint != Vector2.zero
            ? currentSaveData.lastSavePoint
            : Vector3.zero;

        GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        PlayerMovement player = newPlayer.GetComponent<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError("Spawned player has no PlayerMovement!");
            return;
        }

        //  HIER passiert die Magie
        player.RestoreFromSave(currentSaveData.health, currentSaveData.psychometer );

        // Player meldet sich als ready
        RegisterPlayer(player);
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
