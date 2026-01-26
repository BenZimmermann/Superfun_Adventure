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
        CreateManager<GameManager>(gameManagerPrefab);
        CreateManager<GameStateManager>(gameStateManagerPrefab);
        CreateManager<LevelManager>(levelManagerPrefab);
        CreateManager<InputManager>(inputManagerPrefab);
        CreateManager<UIManager>(uiManagerPrefab);
        CreateManager<AudioManager>(audioManagerPrefab);
        CreateManager<SettingsManager>(settingsManagerPrefab);
        CreateManager<SaveManager>(saveManagerPrefab);
    }

    private void CreateManager<T>(GameObject prefab) where T : MonoBehaviour
    {
        if (FindAnyObjectByType<T>() != null) return;

        Instantiate(prefab);
    }
}
