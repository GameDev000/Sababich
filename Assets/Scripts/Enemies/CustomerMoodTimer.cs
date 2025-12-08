using UnityEngine;
using System.Collections;

 /// <summary>
 /// Manages the mood timer for a customer, changing their mood over time and handling their departure.
 /// The customer goes through different mood faces at set intervals and eventually walks away if not served.
 /// </summary>
public class CustomerMoodTimer : MonoBehaviour
{
    [SerializeField] private Sprite[] moodFaces; // Array of sprites representing different mood faces
    [SerializeField] private float interval = 7f; // Time interval to change mood face
    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    [SerializeField] private float duration = 4f; // Duration for walking away animation

    // Internal state variables
    private float elapsed = 0f;
    private float timer = 0f;
    private int currentFace = 0;
    private bool moodStopped = false;

    // Store the starting position to reset later
    private Vector3 startPosition;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer if not assigned at row 12
            
        startPosition = transform.position; // Store the initial position of the customer

        if (moodFaces != null && moodFaces.Length > 0)
            spriteRenderer.sprite = moodFaces[0]; // Set initial mood face
    }

    private void Update()
    {
        if (moodStopped) return;

        timer += Time.deltaTime;
        // Change mood face at intervals
        if (timer >= interval)
        {
            timer = 0f;
            currentFace++; // Move to the next mood face

            if (currentFace < moodFaces.Length)
                spriteRenderer.sprite = moodFaces[currentFace]; // Update to next mood face

            if (currentFace == moodFaces.Length - 1)
            {
                moodStopped = true;
                Debug.Log("Not happy customer! Walking away..."); // Log when the customer is not happy and starts walking away
                StartCoroutine(WalkAwayAndNotify());// Start walking away
            }
        }
    }
    // Call this method when the customer is served correctly
    public void CustomerServed() 
    {
        moodStopped = true;
        currentFace = 0;
        Debug.Log("Happy customer! Leaving...");
        StartCoroutine(WalkAwayAndNotify());
    }
    // Coroutine to handle walking away animation and notify when done
    private IEnumerator WalkAwayAndNotify() 
    {
        elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(10f, 0f, 0f);

        // Move the customer to the right over the duration
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        OnReachedExit(); // Notify that the customer has reached the exit
    }

    private void OnReachedExit()
    {
        Debug.Log("Customer reached exit");

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnCustomerLeftScene(); // Notify the tutorial manager that the customer has left the scene
    }

    // Resets the customer to the initial state for reuse
    public void ResetCustomer()
    {
        moodStopped = false;
        timer = 0f;
        elapsed = 0f;
        currentFace = 0;

        transform.position = startPosition;

        if (spriteRenderer != null && moodFaces != null && moodFaces.Length > 0)
            spriteRenderer.sprite = moodFaces[0]; // Reset to initial mood face
    }
}
