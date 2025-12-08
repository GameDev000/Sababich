using TMPro;
using UnityEngine;

/// <summary>
/// Manages the UI instructions display.
/// </summary>
public class UIInstructions : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI instructionsText; // Reference to the instructions text component

    private void Start()
    {

    }
    /// <summary>
    /// Sets the instructions text to the specified string.
    /// </summary>
    public void SetInstructions(string text)
    {
        if (instructionsText != null)
        {
            instructionsText.ClearMesh();
            instructionsText.text = text;
        }
    }
}
