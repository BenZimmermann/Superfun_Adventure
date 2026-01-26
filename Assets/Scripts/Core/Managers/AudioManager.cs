using UnityEngine;

public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// -global audios
    /// -set master volume
    /// -set efx volume
    /// -mute
    /// -distortion effects
    /// </summary>
    public static AudioManager Instance { get; private set; }
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
    public void SetMasterVolume(float volume)
    {
        Debug.Log($"Setting Master Volume to: {volume}");
    }
    public void ApplyDistortion(float intensity)
    {
        Debug.Log($"Applying Distortion with intensity: {intensity}");
    }
}
