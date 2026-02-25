using UnityEngine;

public class SavePoints : MonoBehaviour
{
    [Header("Save Point Settings")]
    [SerializeField] private int levelToSave = 1;
    [SerializeField] private bool showDebugMessages = true;
    [SerializeField] private bool IsFinishline = false;
    [SerializeField] private GameObject SaveCanvas; 

    // Referenzen zu den Spieler-Komponenten (falls vorhanden)
    private bool hasSaved = false; // Verhindert mehrfaches Speichern
    private PlayerMovement player;
    private float timer = 0f;
    private void OnEnable()
    {
        GameManager.Instance.OnPlayerReady += BindPlayer;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerReady -= BindPlayer;
    }
    private void Update()
    {
        if(!hasSaved && CompareTag("Player"))
        {
            timer += Time.deltaTime;
            SaveCanvas.SetActive(true);

            if (timer >= 2f && hasSaved) // Warte 0.5 Sekunden bevor der Spieler erneut speichern kann
            {
               SaveCanvas.SetActive(false);
                timer = 0f;
            }
        }
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
            SaveCanvas.SetActive(true);
            hasSaved = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            SaveCanvas.SetActive(false);
            hasSaved = false;
            //CorruptionManager.Instance.AddCorruption(10f); // Füge 10 Einheiten Korruption hinzu, wenn der Spieler den Checkpoint verlässt
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
        saveData.distortionLevel = CorruptionManager.Instance.GetCorruptionLevel();

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