using UnityEngine;

public class ButtonPressed : MonoBehaviour
{
    public void OnClose()
    {
        GameManager.Instance.QuitGame();
    }
}
