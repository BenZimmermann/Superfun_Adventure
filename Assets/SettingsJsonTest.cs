using UnityEngine;
using System.IO;

public class SettingsJsonTest : MonoBehaviour
{
    [ContextMenu("Test JSON Serialization")]
    void TestJsonSerialization()
    {
        // Erstelle Test-Daten
        SettingsData testData = new SettingsData();
        testData.masterVolume = 0.8f;
        testData.musicVolume = 0.6f;
        testData.mouseSensitivity = 1.5f;

        // Füge Keybindings hinzu
        testData.SetKeybinding("Move", "<Keyboard>/w");
        testData.SetKeybinding("Jump", "<Keyboard>/space");
        testData.SetKeybinding("Interact", "<Keyboard>/e");

        Debug.Log("=== ORIGINAL DATA ===");
        Debug.Log($"Master Volume: {testData.masterVolume}");
        Debug.Log($"Bindings Count: {testData.bindingKeys.Count}");

        // Serialisiere zu JSON
        string json = JsonUtility.ToJson(testData, true);
        Debug.Log("=== JSON OUTPUT ===");
        Debug.Log(json);

        // Deserialisiere zurück
        SettingsData loadedData = JsonUtility.FromJson<SettingsData>(json);

        Debug.Log("=== LOADED DATA ===");
        Debug.Log($"Master Volume: {loadedData.masterVolume}");
        Debug.Log($"Bindings Count: {loadedData.bindingKeys.Count}");
        Debug.Log($"Move Binding: {loadedData.GetKeybinding("Move")}");
        Debug.Log($"Jump Binding: {loadedData.GetKeybinding("Jump")}");

        // Test: Speichern in Datei
        string path = Path.Combine(Application.persistentDataPath, "test_settings.json");
        File.WriteAllText(path, json);
        Debug.Log($"Saved to: {path}");

        // Test: Laden aus Datei
        string loadedJson = File.ReadAllText(path);
        SettingsData fileData = JsonUtility.FromJson<SettingsData>(loadedJson);
        Debug.Log($"Loaded from file - Move: {fileData.GetKeybinding("Move")}");

        Debug.Log("? JSON Serialization Test PASSED!");
    }
}