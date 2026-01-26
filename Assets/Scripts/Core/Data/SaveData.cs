using UnityEngine;

public class SaveData
{
    public int level;
    public float health;
    public float psychometer;
    public Vector2 lastSavePoint;
    public float distortionLevel;
    public int currentLevel;
    public bool isfinished;

    public SaveData()
    {
        level = 1;
        health = 100f;
        psychometer = 0f;
        lastSavePoint = Vector2.zero;
        distortionLevel = 0f;
        currentLevel = 0;
        isfinished = false;
    }
}
