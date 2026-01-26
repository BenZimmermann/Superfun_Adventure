using UnityEngine;

public class LevelTest : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            LevelManager.Instance.LoadLevel(0);

        if (Input.GetKeyDown(KeyCode.W))
            LevelManager.Instance.LoadLevel(1);

        if (Input.GetKeyDown(KeyCode.E))
            LevelManager.Instance.LoadLevel(2);

        if (Input.GetKeyDown(KeyCode.R))
            LevelManager.Instance.ReloadCurrentLevel();

        if (Input.GetKeyDown(KeyCode.M))
            GameStateManager.Instance.SetState(GameState.MainMenu);
    }
}
