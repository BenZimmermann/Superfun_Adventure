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
    public void StartNewGame()
    {
        Debug.Log("Game Started");
    }
    public void ContinoueGame()
    {
        Debug.Log("Game Continued");
    }
    public void QuitGame()
    {
        Debug.Log("Game Quit");
    }
    public void RestartFromLastSave()
    {
        Debug.Log("Game Restarted from Last Save");
    }
}  
