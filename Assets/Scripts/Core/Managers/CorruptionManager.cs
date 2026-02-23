using UnityEngine;

public class CorruptionManager : MonoBehaviour
{
    private int level;
    private float corruptionLevel;
    private bool isfinished;
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
        }
        Debug.LogWarning($"Corruption Level: {corruptionLevel} | Corruption State: {currentCorruptionState} | Level: {level} | IsBricked: {isfinished}");

    }

    public void AddCorruption(float amount)
    {
        corruptionLevel += amount;
        GameManager.Instance.currentSaveData.distortionLevel = corruptionLevel;
        SetCorruptionState(); // <- optional: keep state in sync when corruption changes
    }

    private void ReadCorruption()
    {
        corruptionLevel = GameManager.Instance.currentSaveData.distortionLevel;
        SetCorruptionState(); // <- optional: keep state in sync when reading corruption
    }
    public float GetCorruptionLevel()
    {
        return corruptionLevel;
    }
    private void ReadLevel()
    {
        level = GameManager.Instance.currentSaveData.level;
    }
    private void ReadIsFinished()
    {
        isfinished = GameManager.Instance.currentSaveData.isfinished;
    }
    public void SetCorruptionState()
    {
        // switch can't evaluate variable conditions, use if/else if instead
        if (corruptionLevel < 25f)
            currentCorruptionState = CorruptionState.Low;
        else if (corruptionLevel < 50f)
            currentCorruptionState = CorruptionState.Medium;
        else if (corruptionLevel < 75f)
            currentCorruptionState = CorruptionState.High;
        else if (corruptionLevel < 90f)
            currentCorruptionState = CorruptionState.Critical;
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