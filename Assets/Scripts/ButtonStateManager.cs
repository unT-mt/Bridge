using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ButtonStateManager : MonoBehaviour
{
    public ContentManager contentManager;
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

        contentManager.OnContentChanged += UpdateButtonStates;
        
        // Initialize the button states (if necessary)
        UpdateButtonStates();
    }

    // Update button states based on the ContentManager or other logic
    public void UpdateButtonStates()
    {
        if(contentManager.currentSequenceState == "first")
        {
            buttonStates["b"] = false;
            buttonStates["m"] = true;
            buttonStates["n"] = true;
        }
        else if(contentManager.currentSequenceState == "last")
        {
            buttonStates["b"] = true;
            buttonStates["m"] = true;
            buttonStates["n"] = false;
        }
        else if(contentManager.currentSequenceState == "mid")
        {
            buttonStates["b"] = true;
            buttonStates["m"] = true;
            buttonStates["n"] = true;
        }
        else if(contentManager.currentSequenceState == "none")
        {
            buttonStates["b"] = false;
            buttonStates["m"] = false;
            buttonStates["n"] = false;
        }

        buttonStates["f"] = true;
        buttonStates["g"] = true;
        buttonStates["h"] = true;
        buttonStates["j"] = true;
        buttonStates["k"] = true;
        buttonStates["l"] = true;

        if(contentManager.currentCategory == "J1") buttonStates["f"] = false;
        if(contentManager.currentCategory == "E1") buttonStates["g"] = false;
        if(contentManager.currentCategory == "J2") buttonStates["h"] = false;
        if(contentManager.currentCategory == "E2") buttonStates["j"] = false;
        if(contentManager.currentCategory == "J3") buttonStates["k"] = false;
        if(contentManager.currentCategory == "E3") buttonStates["l"] = false;
        
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
