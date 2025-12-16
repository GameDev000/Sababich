using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject orderBubbleRoot;
    [SerializeField] private Transform iconsParent;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private List<IngredientIconInfo> ingredientIcons;
    [SerializeField] private CustomerMoodTimer_levels moodTimer;
    public CustomerMoodTimer_levels MoodTimer => moodTimer;

    public CustomerType Data { get; private set; }
    public bool IsLeaving { get; private set; }

    private Dictionary<string, IngredientIconInfo> iconLookup;

    private void Awake()
    {
        iconLookup = new Dictionary<string, IngredientIconInfo>();
        foreach (var info in ingredientIcons)
        {
            if (!string.IsNullOrEmpty(info.id) && info.sprite != null)
            {
                iconLookup[info.id.ToLower()] = info;
            }
        }
    }

    public void Init(CustomerType data)
    {
        Data = data;

        if (spriteRenderer != null && data != null && data.sprite != null)
            spriteRenderer.sprite = data.sprite;

        SetupOrderBubble();

        if (moodTimer != null && data != null)
        {
            if (data.happyFace == null)
                Debug.LogWarning($"CustomerType '{data.name}' has no happyFace assigned!");

            if (data.angryFaces == null || data.angryFaces.Length == 0)
                Debug.LogWarning($"CustomerType '{data.name}' has no angryFaces assigned!");

        moodTimer.Configure(data.happyFace, data.angryFaces);
        }
    }

    public void MarkLeaving()
    {
        IsLeaving = true;
    }

    private void SetupOrderBubble()
    {
        if (orderBubbleRoot == null || iconsParent == null || iconPrefab == null || Data == null)
            return;

        orderBubbleRoot.SetActive(true);

        for (int i = iconsParent.childCount - 1; i >= 0; i--)
            Destroy(iconsParent.GetChild(i).gameObject);

        var ingredients = Data.requiredIngredients;
        if (ingredients == null || ingredients.Count == 0)
            return;

        foreach (var ing in ingredients)
        {
            var id = ing.ToLower();
            if (!iconLookup.TryGetValue(id, out var info))
                continue;

            var icon = Instantiate(iconPrefab, iconsParent);
            icon.transform.localPosition = info.localPosition;
            icon.transform.localScale = info.localScale;

            var sr = icon.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = info.sprite;
        }
    }

    public bool IsOrderCorrect(List<string> ingredients)
    {
        if (Data == null || Data.requiredIngredients == null)
            return false;

        if (ingredients.Count != Data.requiredIngredients.Count)
            return false;

        foreach (var req in Data.requiredIngredients)
            if (!ingredients.Contains(req.ToLower()))
                return false;

        return true;
    }

    public void HideOrderBubble()
    {
        if (orderBubbleRoot != null)
            orderBubbleRoot.SetActive(false);
    }
}

[System.Serializable]
public class IngredientIconInfo
{
    public string id;
    public Sprite sprite;
    public Vector3 localPosition;
    public Vector3 localScale = Vector3.one;
}
