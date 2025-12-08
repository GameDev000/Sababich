// using UnityEngine;
// using System.Collections;

// public class CustomerMoodTimer : MonoBehaviour
// {
//     [SerializeField] private Sprite[] moodFaces;
//     [SerializeField] private float interval = 7f;
//     [SerializeField] private SpriteRenderer spriteRenderer;
//     [SerializeField] private float duration = 4f;
//     private float elapsed = 0f;

//     private float timer = 0f;
//     private int currentFace = 0;
//     private bool moodStopped = false;

//     private void Start()
//     {
//         if (spriteRenderer == null)
//             spriteRenderer = GetComponent<SpriteRenderer>();

//         spriteRenderer.sprite = moodFaces[0];
//     }

//     private void Update()
//     {
//         if (moodStopped) return;

//         timer += Time.deltaTime;

//         if (timer >= interval)
//         {
//             timer = 0f;
//             currentFace++;

//             if (currentFace < moodFaces.Length)
//                 spriteRenderer.sprite = moodFaces[currentFace];

//             if (currentFace == moodFaces.Length - 1)
//             {
//                 moodStopped = true;
//                 Debug.Log("Not happy customer! Walking away...");
//                 StartCoroutine(WalkAwayAndDestroy());
//             }
//         }
//     }
// public void CustomerServed()
// {
//     moodStopped = true;
//     currentFace = 0;
//     if (spriteRenderer != null && moodFaces != null && moodFaces.Length > 0)
//     {
//         spriteRenderer.sprite = moodFaces[0];
//     }
//     Debug.Log("Happy customer! Leaving...");
//     StartCoroutine(WalkAwayAndDestroy());
// }

//     private void OnReachedExit()
//     {   
//         Debug.Log("Customer reached exit");
//         //gameObject.SetActive(false);
//         if (TutorialManager.Instance != null)
//             TutorialManager.Instance.OnCustomerLeftScene();
//     }



//     private IEnumerator WalkAwayAndDestroy()
//     {

//         Vector3 start = transform.position;
//         Vector3 end = start + new Vector3(Vector3.right.x * 10f, 0, 0);

//         while (elapsed < duration)
//         {
//             transform.position = Vector3.Lerp(start, end, elapsed / duration);
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
//         OnReachedExit();
//         //Destroy(gameObject);
//     }

// }


using UnityEngine;
using System.Collections;

public class CustomerMoodTimer : MonoBehaviour
{
    [SerializeField] private Sprite[] moodFaces;
    [SerializeField] private float interval = 7f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float duration = 4f;

    private float elapsed = 0f;
    private float timer = 0f;
    private int currentFace = 0;
    private bool moodStopped = false;

    private Vector3 startPosition;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        startPosition = transform.position;

        if (moodFaces != null && moodFaces.Length > 0)
            spriteRenderer.sprite = moodFaces[0];
    }

    private void Update()
    {
        if (moodStopped) return;

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            currentFace++;

            if (currentFace < moodFaces.Length)
                spriteRenderer.sprite = moodFaces[currentFace];

            if (currentFace == moodFaces.Length - 1)
            {
                moodStopped = true;
                Debug.Log("Not happy customer! Walking away...");
                StartCoroutine(WalkAwayAndNotify());
            }
        }
    }

    public void CustomerServed()
    {
        moodStopped = true;
        currentFace = 0;
        Debug.Log("Happy customer! Leaving...");
        StartCoroutine(WalkAwayAndNotify());
    }

    private IEnumerator WalkAwayAndNotify()
    {
        elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(10f, 0f, 0f);

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        OnReachedExit();
    }

    private void OnReachedExit()
    {
        Debug.Log("Customer reached exit");

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnCustomerLeftScene();
    }

    public void ResetCustomer()
    {
        moodStopped = false;
        timer = 0f;
        elapsed = 0f;
        currentFace = 0;

        transform.position = startPosition;

        if (spriteRenderer != null && moodFaces != null && moodFaces.Length > 0)
            spriteRenderer.sprite = moodFaces[0];
    }
}
