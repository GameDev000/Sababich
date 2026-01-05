using UnityEngine;

/// <summary>
/// Manages the flow of the game through different phases, updating UI instructions accordingly.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    [SerializeField] public UIInstructions uiInstructions; // Reference to the UIInstructions component

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
                uiInstructions.SetInstructions("הכנס חצילים אל עמדת הטיגון .");
                //uiInstructions.SetInstructions("Insert eggplants into the frying station");
                break;

            case GamePhase.FryingEggplant:
                uiInstructions.SetInstructions("המתן להכנת החצילים, \n זהירות שלא ישרפו!");
                //uiInstructions.SetInstructions("Fry the eggplants at the frying station until the timer runs out");
                break;

            case GamePhase.AssembleDish:
                uiInstructions.SetInstructions("כעת נרכיב את המנה, \n שים לב לבקשת הלקוח.");
                //uiInstructions.SetInstructions("Follow the customer's order to assemble the dish - follow the arrows");
                break;

            case GamePhase.ServeCustomer:
                uiInstructions.SetInstructions("תוודא שהמנה מורכבת נכון \nולאחר מכן הגש אותה ללקוח.");
                //uiInstructions.SetInstructions("Check that the dish is assembled correctly and serve it to the customer.");
                break;

            case GamePhase.NextCustomer:
                uiInstructions.SetInstructions("מעולה! \nכעת המתן ללקוח, וחזור על התהליך.");
                //uiInstructions.SetInstructions("The customer is done. Prepare for the next customer and repeat the process.");
                break;

            case GamePhase.GoWashInKitchen:
                uiInstructions.SetInstructions("אופס… התלכלכנו!\n לחץ על החץ למטבח כדי להתנקות.");
                break;

            case GamePhase.WashHands:
                uiInstructions.SetInstructions("השתמש בברז.");
                break;

            case GamePhase.GoBackToStand:
                uiInstructions.SetInstructions("עבודה טובה! \n עכשיו לחץ על החץ כדי להמשיך בעבודה.");
                break;
            case GamePhase.ForbiddenCustomerWarning:
                uiInstructions.SetInstructions("רגע! הלקוח הנוכחי רגיש לגלוטן \n אסור להגיש לו מנה בכלל!");
                break;

        }
    }

    public void ShowTutorialMessage(string text)
    {
        if (uiInstructions == null) return;
        uiInstructions.SetInstructions(text);
    }


}