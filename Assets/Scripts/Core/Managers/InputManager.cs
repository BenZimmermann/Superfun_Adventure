using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    /// <summary>
    /// Disable/enable gameplay input
    /// UI input enable/disable
    /// rebind keybindings
    /// </summary>
    public static InputManager Instance { get; private set; }
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap gameplayActionMap;
    private InputActionMap uiActionMap;

    private bool gameplayInputEnabled = true;
    private bool uiInputEnabled = true;

    private bool GameplayInputEnabled => gameplayInputEnabled;
    private bool UIInputEnabled => uiInputEnabled;

    public System.Action<bool> OnGameplayInputChaged;
    public System.Action<bool> OnUIInputChanged;
    public System.Action<string, string> OnRebindComplete;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeInputActions();
    }
    private void OnEnable()
    {
        if(GameManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged += HandleGameStateChanged;
        }
        if(SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged += ApplyInputSettings;
        }
        EnableGameplay();
        EnableUI();
    }
    private void OnDisable()
    {
        if(GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged -= ApplyInputSettings;
        }
    }
    private void InitializeInputActions()
    {
        if (inputActions == null)
            return;
        gameplayActionMap = inputActions.FindActionMap("Gameplay");
        uiActionMap = inputActions.FindActionMap("UI");

        LoadBindingsFromSettings();
    }
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Boot:
                DisableAllInput();
                break;
            case GameState.MainMenu:
                DisableGameplay();
                EnableUI();
                break;
            case GameState.Playing:
                EnableGameplay();
                DisableUI();
                break;
            //case GameState.Paused:
            //    EnableGameplay();
            //    EnableUI();
            //    break;
            case GameState.BossFight:
                EnableGameplay();
                EnableUI();
                break;
            case GameState.GameOver:
                DisableGameplay();
                EnableUI();
                break;
        }
    }
    public void EnableGameplay()
    {
        if(gameplayActionMap == null)return;

            gameplayActionMap.Enable();
            gameplayInputEnabled = true;
            OnGameplayInputChaged?.Invoke(true);
    }
    public void DisableGameplay()
    {
        if (gameplayActionMap == null) return;

        gameplayActionMap.Disable();
        gameplayInputEnabled = false;
        OnGameplayInputChaged?.Invoke(false);
    }
    public void EnableUI()
    {
        if(uiActionMap == null) return;
        
        uiActionMap.Enable();
        uiInputEnabled = true;
        OnUIInputChanged?.Invoke(true);
    }
    public void DisableUI()
    {
        if (uiActionMap == null) return;

        uiActionMap.Disable();
        uiInputEnabled = false;
        OnUIInputChanged?.Invoke(false);
    }
    public void DisableAllInput()
    {
        DisableGameplay();
        DisableUI();
    }
    /// <summary>
    /// Startet Rebinding für eine Action
    /// </summary>
    public void StartRebinding(string actionName, int bindingIndex = 0, System.Action<string> onComplete = null)
    {
        InputAction action = GetAction(actionName);
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found!");
            return;
        }

        // Deaktiviere Action während Rebinding
        action.Disable();

        // Starte Rebinding Operation
        var rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(operation =>
            {
                // Speichere neues Binding
                string newBinding = action.bindings[bindingIndex].effectivePath;
                SaveBindingToSettings(actionName, newBinding);

                // Re-aktiviere Action
                action.Enable();

                // Cleanup
                operation.Dispose();

                Debug.Log($"Rebound '{actionName}' to: {newBinding}");
                OnRebindComplete?.Invoke(actionName, newBinding);
                onComplete?.Invoke(newBinding);
            })
            .OnCancel(operation =>
            {
                // Re-aktiviere Action
                action.Enable();
                operation.Dispose();
                Debug.Log($"Rebinding cancelled for '{actionName}'");
            })
            .Start();
    }

    /// <summary>
    /// Setzt eine Action auf Default zurück
    /// </summary>
    public void ResetBinding(string actionName, int bindingIndex = 0)
    {
        InputAction action = GetAction(actionName);
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found!");
            return;
        }

        action.RemoveBindingOverride(bindingIndex);
        RemoveBindingFromSettings(actionName);

        Debug.Log($"Reset binding for '{actionName}' to default");
    }

    /// <summary>
    /// Setzt alle Bindings zurück
    /// </summary>
    public void ResetAllBindings()
    {
        if (gameplayActionMap != null)
        {
            gameplayActionMap.RemoveAllBindingOverrides();
        }

        if (uiActionMap != null)
        {
            uiActionMap.RemoveAllBindingOverrides();
        }

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ClearAllKeybindings();
            SettingsManager.Instance.SaveSettings();
        }

        Debug.Log("All bindings reset to default");
    }

    // ===== SETTINGS INTEGRATION =====

    /// <summary>
    /// Lädt Bindings aus SettingsManager
    /// </summary>
    private void LoadBindingsFromSettings()
    {
        if (SettingsManager.Instance == null) return;

        var bindings = SettingsManager.Instance.GetAllKeybindings();

        foreach (var binding in bindings)
        {
            InputAction action = GetAction(binding.Key);
            if (action != null)
            {
                // Überschreibe Binding
                action.ApplyBindingOverride(0, binding.Value);
            }
        }

        Debug.Log($"Loaded {bindings.Count} bindings from settings");
    }

    /// <summary>
    /// Speichert Binding in Settings
    /// </summary>
    private void SaveBindingToSettings(string actionName, string bindingPath)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetKeybinding(actionName, bindingPath);
            SettingsManager.Instance.SaveSettings();
        }
    }

    /// <summary>
    /// Entfernt Binding aus Settings
    /// </summary>
    private void RemoveBindingFromSettings(string actionName)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.RemoveKeybinding(actionName);
            SettingsManager.Instance.SaveSettings();
        }
    }

    /// <summary>
    /// Wendet Input Settings an (z.B. Mouse Sensitivity)
    /// </summary>
    private void ApplyInputSettings()
    {

        Debug.Log("Input settings applied");
    }

    // ===== HELPER METHODS =====

    /// <summary>
    /// Holt eine Input Action
    /// </summary>
    private InputAction GetAction(string actionName)
    {
        if (gameplayActionMap != null)
        {
            var action = gameplayActionMap.FindAction(actionName);
            if (action != null) return action;
        }

        if (uiActionMap != null)
        {
            var action = uiActionMap.FindAction(actionName);
            if (action != null) return action;
        }

        return null;
    }

    /// <summary>
    /// Gibt das aktuelle Binding einer Action zurück
    /// </summary>
    public string GetCurrentBinding(string actionName, int bindingIndex = 0)
    {
        InputAction action = GetAction(actionName);
        if (action == null) return "";

        if (bindingIndex < action.bindings.Count)
        {
            return action.bindings[bindingIndex].effectivePath;
        }

        return "";
    }

    /// <summary>
    /// Gibt den Display-Namen eines Bindings zurück
    /// </summary>
    public string GetBindingDisplayString(string actionName, int bindingIndex = 0)
    {
        InputAction action = GetAction(actionName);
        if (action == null) return "";

        if (bindingIndex < action.bindings.Count)
        {
            return action.GetBindingDisplayString(bindingIndex);
        }

        return "";
    }

    // ===== PUBLIC API FÜR ACTIONS =====

    /// <summary>
    /// Prüft ob eine Action gedrückt wurde
    /// </summary>
    public bool GetActionTriggered(string actionName)
    {
        InputAction action = GetAction(actionName);
        return action != null && action.triggered;
    }

    /// <summary>
    /// Prüft ob eine Action gehalten wird
    /// </summary>
    public bool GetActionPressed(string actionName)
    {
        InputAction action = GetAction(actionName);
        return action != null && action.IsPressed();
    }

    /// <summary>
    /// Gibt den Wert einer Action zurück
    /// </summary>
    public float GetActionValue(string actionName)
    {
        InputAction action = GetAction(actionName);
        if (action == null) return 0f;

        return action.ReadValue<float>();
    }

    /// <summary>
    /// Gibt den Vector2 Wert einer Action zurück
    /// </summary>
    public Vector2 GetActionVector2(string actionName)
    {
        InputAction action = GetAction(actionName);
        if (action == null) return Vector2.zero;

        return action.ReadValue<Vector2>();
    }
}
