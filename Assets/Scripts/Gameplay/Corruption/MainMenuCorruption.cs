using UnityEngine;
using System.Collections;
using static CorruptionManager;

public class MainMenuCorruption : MonoBehaviour
{
    [SerializeField] private GameObject corrupted;
    [SerializeField] private GameObject medium;
    [SerializeField] private GameObject high;

    private bool isQuitting = false; // Verhindert mehrfaches Starten der Coroutine

    private void Update()
    {
        if (CorruptionManager.Instance == null) return;

        // 1. Finish Logik (mit Sicherheitsabfrage)
        if (CorruptionManager.Instance.isfinished && !isQuitting)
        {
            isQuitting = true;
            corrupted.SetActive(true);
            StartCoroutine(QuitTime());
        }

        // 2. State Logik: Wir switchen direkt über den Enum!
        switch (CorruptionManager.Instance.currentCorruptionState)
        {
            case CorruptionState.Medium:
                if (!medium.activeSelf) medium.SetActive(true);
                break;

            case CorruptionState.High:
                if (!high.activeSelf) high.SetActive(true);
                break;

                // Optional: Was passiert bei "Default" oder "Low"?
                // case CorruptionState.Low: 
                //    break;
        }
    }

    private IEnumerator QuitTime()
    {
        yield return new WaitForSeconds(10f);
        GameManager.Instance.QuitGame();
    }
}