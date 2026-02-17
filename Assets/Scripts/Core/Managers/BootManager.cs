using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    [SerializeField] private string startSceneName = "MainMenu";

    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject gameStateManagerPrefab;
    [SerializeField] private GameObject levelManagerPrefab;
    [SerializeField] private GameObject inputManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject settingsManagerPrefab;
    [SerializeField] private GameObject saveManagerPrefab;
    [SerializeField] private GameObject corruptionManagerPrefab;

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
        CreateManager<SaveManager>(saveManagerPrefab);
        CreateManager<GameStateManager>(gameStateManagerPrefab);
        CreateManager<LevelManager>(levelManagerPrefab);
        CreateManager<CorruptionManager>(corruptionManagerPrefab);
        CreateManager<GameManager>(gameManagerPrefab);
        CreateManager<InputManager>(inputManagerPrefab);
        CreateManager<UIManager>(uiManagerPrefab);
        CreateManager<AudioManager>(audioManagerPrefab);
        CreateManager<SettingsManager>(settingsManagerPrefab);


        GameStateManager.Instance.SetState(GameState.MainMenu);
    }

    private void CreateManager<T>(GameObject prefab) where T : MonoBehaviour
    {
        if (FindAnyObjectByType<T>() != null) return;

        Instantiate(prefab);

    }
}
