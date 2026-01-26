using UnityEngine;

public class LevelManager : MonoBehaviour
{
    /// <summary>
    /// -load levels via Index
    /// -reload current level
    /// -hold current level info
    /// </summary>
    public static LevelManager Instance { get; private set; }
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
    public void LoadLevel(int levelName)
    {
        Debug.Log($"Loading Level: {levelName}");
    }
    private void ReloadCurrentLevel()
    {
        Debug.Log("Reloading Current Level");
    }
}
