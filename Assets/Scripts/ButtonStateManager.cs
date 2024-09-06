using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ButtonStateManager : MonoBehaviour
{
    // Dictionary to store the button states
    private Dictionary<string, bool> buttonStates = new Dictionary<string, bool>()
    {
        { "f", false },
        { "g", false },
        { "h", false },
        { "j", false },
        { "k", false },
        { "l", false },
        { "b", false },
        { "m", false },
        { "n", false }
    };

    // File path to save the JSON file (desktop path)
    private string jsonFilePath;

    private void Start()
    {
        // Set the file path to desktop
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        jsonFilePath = Path.Combine(desktopPath, "wwo", "stats.json");

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(jsonFilePath));

        // Initialize the button states (if necessary)
        UpdateButtonStates();
    }

    // Update button states based on the ContentManager or other logic
    public void UpdateButtonStates()
    {
        // Example: Enable only F, G, and J buttons
        buttonStates["f"] = true;
        buttonStates["g"] = true;
        buttonStates["h"] = false;
        buttonStates["j"] = true;
        buttonStates["k"] = false;
        buttonStates["l"] = false;
        buttonStates["b"] = false;
        buttonStates["m"] = false;
        buttonStates["n"] = false;

        // Save the updated states to a JSON file
        SaveButtonStatesToJson();
    }

    // Save the button states to the JSON file
    private void SaveButtonStatesToJson()
    {
        // Serialize the dictionary to JSON
        string jsonContent = JsonConvert.SerializeObject(new { switch_led = buttonStates }, Formatting.Indented);

        // Write the JSON content to the file
        File.WriteAllText(jsonFilePath, jsonContent);

        Debug.Log("Button states saved to: " + jsonFilePath);
    }
}
