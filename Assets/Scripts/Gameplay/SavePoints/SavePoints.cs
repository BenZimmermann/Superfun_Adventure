using UnityEngine;

public class SavePoints : MonoBehaviour
{
    [Header("Save Point Settings")]
    [SerializeField] private int levelToSave = 1;
    [SerializeField] private bool showDebugMessages = true;
    [SerializeField] private bool IsFinishline = false;

    // Referenzen zu den Spieler-Komponenten (falls vorhanden)
    private bool hasSaved = false; // Verhindert mehrfaches Speichern
    private PlayerMovement player;
    private void OnEnable()
    {
        GameManager.Instance.OnPlayerReady += BindPlayer;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerReady -= BindPlayer;
    }
    private void BindPlayer(PlayerMovement p)
    {
        player = p;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasSaved)
        {
            SaveGameAtCheckpoint(other.transform);
            hasSaved = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasSaved = false;
        }
    }

    private void SaveGameAtCheckpoint(Transform playerTransform)
    {
        SaveData saveData = new SaveData();

        // Speichere Level-Informationen
        saveData.level = levelToSave;
        saveData.currentLevel = levelToSave;
        saveData.health = player.currentHealth;
        saveData.psychometer = player.currentPsychosis;

        if (IsFinishline)
        {
            saveData.currentLevel++;
            Debug.Log("$Finishline reached! {saveData.level}");
            GameManager.Instance.PauseGame();
            GameStateManager.Instance.SetState(GameState.LevelEnd);

        }
        else
        {
            // Speichere Spieler-Position als letzten Save Point nur wenn der checkpoint nicht die finishline ist
            saveData.lastSavePoint = playerTransform.position;
        }
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(saveData);

            if (showDebugMessages)
            {
                Debug.Log($"=== GAME SAVED AT CHECKPOINT ===");
                Debug.Log($"Level: {saveData.level}");
                Debug.Log($"Health: {saveData.health}");
                Debug.Log($"Psychometer: {saveData.psychometer}");
                Debug.Log($"Position: {saveData.lastSavePoint}");
                Debug.Log($"Distortion: {saveData.distortionLevel}");
                Debug.Log($"================================");
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>()?.bounds.size ?? Vector3.one);
    }
}