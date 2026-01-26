using UnityEngine;

public class InputManager : MonoBehaviour
{
    /// <summary>
    /// Disable/enable gameplay input
    /// UI input enable/disable
    /// rebind keybindings
    /// </summary>
    public static InputManager Instance { get; private set; }
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
    public void EnableGameplay()
    {
        Debug.Log("Gameplay Input Enabled");
    }
    public void EnableUI()
    {
        Debug.Log("UI Input Enabled");
    }
    public void RebindAction()
    {
       Debug.Log("Action Rebound");
    }
}
