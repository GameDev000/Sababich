using UnityEngine;

/// <summary>
/// Manages the flow of the game through different phases, updating UI instructions accordingly.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    [SerializeField] private UIInstructions uiInstructions; // Reference to the UIInstructions component

    public GamePhase CurrentPhase { get; private set; } // Current phase of the game

    private void Start()
    {
        SetPhase(GamePhase.AddRowEggplant); // Start the game in the AddRowEggplant phase
    }

    /// <summary>
    /// Sets the current game phase and updates the UI instructions accordingly.
    /// </summary>
    public void SetPhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;// Update the current phase

        if (uiInstructions == null) return;

        switch (newPhase) // Update UI instructions based on the current phase, will be on hebrew later
        {
            case GamePhase.AddRowEggplant:
                //uiInstructions.SetInstructions("הכנס חצילים אל עמדת הטיגון כדי להתחיל לטגן אותם.");
                uiInstructions.SetInstructions("Insert eggplants into the frying station");
                break;

            case GamePhase.FryingEggplant:
                //uiInstructions.SetInstructions("טגן את החצילים בעמדת הטיגון עד שהשעון מסתיים – אל תשרוף אותם!");
                uiInstructions.SetInstructions("Fry the eggplants at the frying station until the timer runs out");
                break;

            case GamePhase.AssembleDish:
                //uiInstructions.SetInstructions("הרכב את המנה – הוסף חציל ותוספות לפי ההזמנה של הלקוח.");
                uiInstructions.SetInstructions("Follow the customer's order to assemble the dish - follow the arrows");
                break;

            case GamePhase.ServeCustomer:
                //uiInstructions.SetInstructions("בדוק שהמנה מורכבת נכון והגש אותה ללקוח.");
                uiInstructions.SetInstructions("Check that the dish is assembled correctly and serve it to the customer.");
                break;

            case GamePhase.NextCustomer:
                //uiInstructions.SetInstructions("הלקוח סיים. התכונן ללקוח הבא וחזור על התהליך.");
                uiInstructions.SetInstructions("The customer is done. Prepare for the next customer and repeat the process.");
                break;
        }
    }
}