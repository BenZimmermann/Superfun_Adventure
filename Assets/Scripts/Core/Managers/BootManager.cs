using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    [SerializeField] private string startSceneName = "MainMenu";
    private static bool hasBooted;

    private void Awake()
    {
        if (hasBooted)
        {
            Destroy(gameObject);
            return;
        }

        hasBooted = true;
        DontDestroyOnLoad(gameObject);

        InitializeManagers();
        SceneManager.LoadScene(startSceneName);
    }
    /// <summary>
    /// Initializes all necessary managers for the game, in a sepeate scene to avoid circular dependencies.
    /// </summary>
    private void InitializeManagers()
    {
        CreateGameManager();
        CreateGameStateManager();
        CreateLevelManager();
        CreateInputManager();
        CreateUIManager();
        CreateAudioManager();
        CreateSettingsManager();
        CreateSaveManager();
        //CreateCorruptionManager();
    }

    private void CreateGameManager()
    {
        if (GameManager.Instance != null) return;
        Debug.Log("Creating GameManager");
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    private void CreateGameStateManager()
    {
        if (GameStateManager.Instance != null) return;
        Debug.Log("Creating GameStateManager");
        new GameObject("GameStateManager").AddComponent<GameStateManager>();
    }
    private void CreateLevelManager()
    {
        if (LevelManager.Instance != null) return;
        Debug.Log("Creating LevelManager");
        new GameObject("LevelManager").AddComponent<LevelManager>();
    }
    private void CreateInputManager()
    {
        if (InputManager.Instance != null) return;
        Debug.Log("Creating InputManager");
        new GameObject("InputManager").AddComponent<InputManager>();
    }

    private void CreateUIManager()
    {
        if (UIManager.Instance != null) return;
        Debug.Log("Creating UIManager");
        new GameObject("UIManager").AddComponent<UIManager>();
    }

    private void CreateAudioManager()
    {
        if (AudioManager.Instance != null) return;
        new GameObject("AudioManager").AddComponent<AudioManager>();
    }

    private void CreateSettingsManager()
    {
        if (SettingsManager.Instance != null) return;
        Debug.Log("Creating SettingsManager");
        new GameObject("SettingsManager").AddComponent<SettingsManager>();
    }

    private void CreateSaveManager()
    {
        if (SaveManager.Instance != null) return;
        Debug.Log("Creating SaveManager");
        new GameObject("SaveManager").AddComponent<SaveManager>();
    }

    //private void CreateCorruptionManager()
    //{
    //    if (CorruptionManager.Instance != null) return;
    //    new GameObject("CorruptionManager").AddComponent<CorruptionManager>();
    //}
}
