using UnityEngine;

public class CorruptionManager : MonoBehaviour
{
    private int level;
    public float corruptionLevel;
    public bool isfinished;
    public CorruptionState currentCorruptionState;

    public static CorruptionManager Instance { get; private set; }

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
    private void Update()
    {
        if (GameManager.Instance.currentSaveData != null)
        {
           
            ReadCorruption(); // <- keep corruptionLevel in sync with save data
            ReadLevel();
            ReadIsFinished();
        }
        Debug.LogWarning($"Corruption Level: {corruptionLevel} | Corruption State: {currentCorruptionState} | Level: {level} | IsBricked: {isfinished}");

    }

    private void ReadCorruption()
    {
        SetCorruptionState(); // <- optional: keep state in sync when reading corruption
    }
    public float GetCorruptionLevel()
    {
        return corruptionLevel;
    }
    private void ReadLevel()
    {
        level = GameManager.Instance.currentSaveData.currentLevel;
    }
    private void ReadIsFinished()
    {
        isfinished = GameManager.Instance.currentSaveData.isfinished;
    }
    public void SetCorruptionState()
    {
        // switch can't evaluate variable conditions, use if/else if instead
            if (level == 1)
            currentCorruptionState = CorruptionState.Medium;
        else if (level == 2)
            currentCorruptionState = CorruptionState.High;
        else
            currentCorruptionState = CorruptionState.None;
    }

    public enum CorruptionState
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }
}