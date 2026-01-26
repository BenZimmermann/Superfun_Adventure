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
    public void OpenMenu(string menuName)
    {
        Debug.Log($"Opening Menu: {menuName}");
    }
    public void CloseCurrentMenu(string menuName)
    {
        Debug.Log($"Opening Menu: {menuName}");
    }
    public void IsMenuOpen(string menuName)
    {
        Debug.Log($"Is Menu Open: {menuName}?");
    }
}
