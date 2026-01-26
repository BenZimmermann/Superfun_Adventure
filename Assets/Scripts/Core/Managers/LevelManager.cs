using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    /// <summary>
    /// -load levels via Index
    /// -reload current level
    /// -hold current level info
    /// </summary>
    public static LevelManager Instance { get; private set; }
    [SerializeField] private string[] levelSceneNames;

    private int currentLevelIndex = -1;
    public int CurrentLevelIndex => currentLevelIndex;

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
    private void OnEnable()
    {
        GameStateManager.Instance.OnStateChanged += HandleGameStateChanged;
    }
    private void OnDisable()
    {
        if(GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        if(newState == GameState.MainMenu)
        {
            LoadSceneByName("MainMenu");
        }
    }
    public void LoadLevel(int levelIndex)
    {
        if(levelIndex < 0 || levelIndex >= levelSceneNames.Length)
        {
            Debug.LogWarning(levelSceneNames.Length.ToString());
            Debug.LogError("Level Index out of bounds!");
            return;
        }
        currentLevelIndex = levelIndex;
        LoadSceneByName(levelSceneNames[levelIndex]);
    }
    public void ReloadCurrentLevel()
    {
        if(currentLevelIndex < 0)
        {
            Debug.LogWarning("No level loaded to reload!");
            return;
        }
        LoadSceneByName(levelSceneNames[currentLevelIndex]);
    }
    public void LoadSceneByName(string sceneName)
    {
        Debug.Log($"Loading Scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
