using UnityEngine;
/// <summary>
///  Handles player interactions with the trash can to discard selected ingredients 
/// and provides visual feedback when hovered over.
/// </summary>
public class TrashCan : MonoBehaviour
{
    [Header("Dependencies")] // Reference to the SelectionList manager
    [SerializeField] private SelectionList selectionListManager;// Reference to the SelectionList component

    [Header("Visuals")]
    [SerializeField] private Sprite closedSprite;// Sprite when the trash can is closed
    [SerializeField] private Sprite openSprite;// Sprite when the trash can is open

    private SpriteRenderer myRenderer;

    private void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();

        if (selectionListManager == null)
        {
            selectionListManager = SelectionList.Instance;
        }

        if (myRenderer != null && closedSprite != null)
        {
            myRenderer.sprite = closedSprite;
        }
    }

    private void OnMouseEnter()// Change sprite to open when hovered over
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
        if (myRenderer != null && openSprite != null)
            myRenderer.sprite = openSprite;
    }

    private void OnMouseExit()// Change sprite back to closed when not hovered over
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
        if (myRenderer != null && closedSprite != null)
            myRenderer.sprite = closedSprite;
    }

    private void OnMouseDown()// Discard selected ingredients when clicked
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
        if (selectionListManager != null)
        {
            Debug.Log("Pita thrown to trash!");

            selectionListManager.ClearIngredients();

        }
        else
        {
            Debug.LogError("TrashCan: SelectionList reference is missing!");
        }
    }
}