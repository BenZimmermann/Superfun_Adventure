using UnityEngine;
using System.Collections;

public class CrashManager : MonoBehaviour
{
    [Header("Crash Settings")]
    [SerializeField] private bool popupSpawner = false;
    [SerializeField] private GameObject[] crashMessagePrefabs;
    [SerializeField] private int crashMessageLimit = 10;
    [SerializeField] private float spawnDelay = 0.05f;

    [Header("UI Root")]
    [SerializeField] private Canvas targetCanvas;

    private void Awake()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.GameOver && popupSpawner && GameManager.Instance.currentSaveData.currentLevel >= 1)
        {
            StartCoroutine(SpawnCrashPopups());
        }
        else if(newState == GameState.LevelEnd)
        { 
            CreateSingleCrashMessage();
        }
    }

    private IEnumerator SpawnCrashPopups()
    {
        for (int i = 0; i < crashMessageLimit; i++)
        {
            CreatePopup();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void CreatePopup()
    {
        if (crashMessagePrefabs.Length == 0 || targetCanvas == null)
            return;

        GameObject prefab =
            crashMessagePrefabs[Random.Range(0, crashMessagePrefabs.Length)];

        GameObject popup = Instantiate(prefab, targetCanvas.transform);

        RectTransform rt = popup.GetComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(
            Random.Range(-Screen.width * 0.3f, Screen.width * 0.3f),
            Random.Range(-Screen.height * 0.3f, Screen.height * 0.3f)
        );

        popup.SetActive(true);
    }
    //spawns the popup once in the middle of the screen
    private void CreateSingleCrashMessage()
    {
        if(crashMessagePrefabs.Length == 0 || targetCanvas == null)
            return;
        GameObject prefab =
            crashMessagePrefabs[Random.Range(0, crashMessagePrefabs.Length)];
        GameObject popup = Instantiate(prefab, targetCanvas.transform);
        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        popup.SetActive(true);

    }
}
