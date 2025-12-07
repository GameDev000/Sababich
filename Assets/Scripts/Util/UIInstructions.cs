using TMPro;
using UnityEngine;

public class UIInstructions : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI instructionsText;

    private void Start()
    {
    
    }
    public void SetInstructions(string text)
    {
        if (instructionsText != null)
        {
            instructionsText.ClearMesh();
            instructionsText.text = text;
        }

            
    }
}
