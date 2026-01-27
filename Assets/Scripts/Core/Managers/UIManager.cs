using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    /// <summary>
    ///    UI Management(not specific menus!)
    ///    opens and closes menus
    ///    disable multiple menus at once
    ///    HuD binden
    /// </summary>
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject bossHealthUI;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject hudUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject corupptedUI;

    private Dictionary<string, GameObject> menus = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeMenus();
    }
    public void OnEnable()
    {
        if(GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged += HandleGameStateChanged;
        }
    }
    private void OnDisable()
    {
        if(GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }
    private void InitializeMenus()
    {
        if(mainMenuUI != null) menus.Add("MainMenu", mainMenuUI);
        if (pauseMenuUI != null) menus.Add("PauseMenu", pauseMenuUI);
        if (bossHealthUI != null) menus.Add("BossHealth", bossHealthUI);
        if (deathUI != null) menus.Add("DeathMenu", deathUI);
        if (hudUI != null) menus.Add("HUD", hudUI);
        if (settingsUI != null) menus.Add("SettingsMenu", settingsUI);
        if (corupptedUI != null) menus.Add("CorruptedMenu", corupptedUI);

        CloseAllMenus();
    }
    private void HandleGameStateChanged(GameState oldstate, GameState newstate)
    {
        CloseAllMenus();
        switch(newstate)
        {
            case GameState.Boot:
                break;
            case GameState.MainMenu:
                OpenMenu("MainMenu");
                break;
            case GameState.Playing:
                OpenMenu("HUD");
                break;
            case GameState.Paused:
                OpenMenu("PauseMenu");
                break;
            case GameState.BossFight:
                OpenMenu("BossHealth");
                break;
            case GameState.GameOver:
                OpenMenu("DeathMenu");
                break;
            case GameState.Corrupted:
                OpenMenu("CorruptedMenu");
                break;
            case GameState.Settings:
                OpenMenu("SettingsMenu");
                break;
        }
    }
    public void OpenMenu(string menuName)
    {
        if (!menus.ContainsKey(menuName))
            {
                return;
            }
        if (menus[menuName] != null)
            { 
                menus[menuName].SetActive(true);
                Debug.Log($"Opening Menu: {menuName}");
            }
    }
    public void CloseCurrentMenu(string menuName)
    {
        if (menus.ContainsKey(menuName))
        {
            return;
        }
        if (menus[menuName] != null)
        {
            menus[menuName].SetActive(false);
            Debug.Log($"Opening Menu: {menuName}");
        }
    }
    public void CloseAllMenus()
    {
        foreach (var menu in menus.Values)
        {
            if (menu != null)
            {
                menu.SetActive(false);
            }
        }
    }
    public bool IsMenuOpen(string menuName)
    {
        if(!menus.ContainsKey(menuName))
        {
            return false;
        }
        return menus[menuName] != null && menus[menuName].activeSelf;
    }
    public void BindHUD(GameObject hud)
    {
        hudUI = hud;
        if (!menus.ContainsKey("HUD"))
        {
            menus.Add("HUD", hudUI);
        }
        else
        {
            menus["HUD"] = hudUI;
        }
    }
    public void ShowHUD()
    {
        OpenMenu("HUD");
    }
    public void HideHUD()
    {
        CloseCurrentMenu("HUD");
    }
}
