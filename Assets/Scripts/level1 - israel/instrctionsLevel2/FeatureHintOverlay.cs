// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class FeatureHintOverlay : MonoBehaviour
// {
//     [SerializeField] private CanvasGroup canvasGroup; // blocksRaycasts=true
//     [SerializeField] private RectTransform board;
//     [SerializeField] private TextMeshProUGUI text;
//     [SerializeField] private Button okButton;
//     [SerializeField] private Vector2 offset = new Vector2(0, 120f);
//     [SerializeField] private Canvas parentCanvas;

//     private Camera cam;
//     private Transform target;
//     private System.Action onOk;

//     private void Awake()
//     {
//         cam = Camera.main;
//         okButton.onClick.AddListener(() => onOk?.Invoke());
//         Hide();
//     }

//     private void Update()
//     {
//         if (target == null) return;
//         Vector3 screenPos = cam.WorldToScreenPoint(target.position);
//         board.position = (Vector2)screenPos + offset;
//     }

//     public void Show(string message, Transform targetTransform, System.Action onOkClicked)
//     {
//         target = targetTransform;
//         onOk = onOkClicked;

//         text.text = message;

//         gameObject.SetActive(true);
//         canvasGroup.alpha = 1f;
//         canvasGroup.interactable = true;
//         canvasGroup.blocksRaycasts = true;
//     }

//     public void Hide()
//     {
//         target = null;
//         onOk = null;

//         canvasGroup.alpha = 0f;
//         canvasGroup.interactable = false;
//         canvasGroup.blocksRaycasts = false;
//         gameObject.SetActive(false);
//     }
// }
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeatureHintOverlay : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private CanvasGroup canvasGroup; // blocksRaycasts=true
    [SerializeField] private RectTransform board;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button okButton;

    [Header("Positioning")]
    [SerializeField] private Vector2 offset = new Vector2(0, 120f);
    [SerializeField] private Canvas parentCanvas; // <-- drag your Canvas here (Screen Space - Camera)

    private Camera cam;
    private Transform target;
    private System.Action onOk;

    private void Awake()
    {
        // Use the Canvas camera if available (Screen Space - Camera), otherwise fallback to main.
        cam = (parentCanvas != null && parentCanvas.worldCamera != null)
            ? parentCanvas.worldCamera
            : Camera.main;

        if (okButton != null)
            okButton.onClick.AddListener(() => onOk?.Invoke());

        Hide();
    }

    private void Update()
    {
        if (target == null || board == null || parentCanvas == null) return;

        // If camera is missing for some reason, fallback safely.
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Convert world target -> screen point
        Vector3 screenPos3 = cam.WorldToScreenPoint(target.position);

        // If target is behind camera, don't move (prevents crazy positions)
        if (screenPos3.z < 0f) return;

        // Convert screen point -> local point on the Canvas
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos3,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
            out Vector2 localPoint
        );

        // Move board in canvas local space (correct for Screen Space - Camera)
        board.anchoredPosition = localPoint + offset;

        // Optional: clamp so it won't go off-screen
        float pad = 80f;
        Vector2 p = board.anchoredPosition;
        p.x = Mathf.Clamp(p.x, -canvasRect.rect.width / 2f + pad, canvasRect.rect.width / 2f - pad);
        p.y = Mathf.Clamp(p.y, -canvasRect.rect.height / 2f + pad, canvasRect.rect.height / 2f - pad);
        board.anchoredPosition = p;
    }

    public void Show(string message, Transform targetTransform, System.Action onOkClicked)
    {
        target = targetTransform;
        onOk = onOkClicked;

        if (text != null)
            text.text = message;

        // Ensure camera is correct even if this object was created before canvas camera exists
        if (cam == null)
        {
            cam = (parentCanvas != null && parentCanvas.worldCamera != null)
                ? parentCanvas.worldCamera
                : Camera.main;
        }

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        target = null;
        onOk = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}
