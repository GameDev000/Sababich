
// using System.Collections.Generic;
// using UnityEngine;

// /// <summary>
// /// Builds and updates the visual representation of the pita
// /// based on the currently selected ingredients.
// /// </summary>
// public class PitaBuilder : MonoBehaviour
// {
//     [Header("Pita Sprites")]
//     [SerializeField] private SpriteRenderer frontPita;

//     [Header("Front pita variants (sauces)")]
//     [SerializeField] private Sprite frontDefault;
//     [SerializeField] private Sprite frontTahini;
//     [SerializeField] private Sprite frontAmba;
//     [SerializeField] private Sprite frontTahiniAndAmba;

//     [Header("Inner ingredient layers")]
//     [SerializeField] private SpriteRenderer eggplantLayer;
//     [SerializeField] private SpriteRenderer eggLayer;
//     [SerializeField] private SpriteRenderer saladLayer;
//     [SerializeField] private SpriteRenderer chipsLayer;
//     [SerializeField] private SpriteRenderer soyLayer;

//     [Header("Inner ingredient sprites")]
//     [SerializeField] private Sprite eggplantSprite;
//     [SerializeField] private Sprite eggSprite;
//     [SerializeField] private Sprite saladSprite;
//     [SerializeField] private Sprite chipsSprite;
//     [SerializeField] private Sprite soySprite;

//     private readonly List<SpriteRenderer> allPitaRenderers = new List<SpriteRenderer>();

//     private void Awake()
//     {
//         CacheAllPitaRenderers();
//     }

//     private void Start()
//     {
//         ClearAllPitaVisuals();
//     }

//     /// <summary>
//     /// Rebuilds the pita visuals according to the selected ingredients list.
//     /// Called every time the selection changes.
//     /// </summary>
//     public void BuildFromSelection(List<string> ingredients)
//     {
//         ClearAllPitaVisuals();

//         if (ingredients == null || ingredients.Count == 0)
//             return;

//         bool hasTahini = false;
//         bool hasAmba = false;
//         bool hasPita = false;

//         foreach (var rawIng in ingredients)
//         {
//             if (string.IsNullOrWhiteSpace(rawIng))
//                 continue;

//             string ing = rawIng.ToLowerInvariant();

//             if (ing == "pitta")
//             {
//                 hasPita = true;
//                 continue;
//             }

//             switch (ing)
//             {
//                 case "eggplant":
//                     EnableLayer(eggplantLayer, eggplantSprite);
//                     break;

//                 case "egg":
//                     EnableLayer(eggLayer, eggSprite);
//                     break;

//                 case "salad":
//                     EnableLayer(saladLayer, saladSprite);
//                     break;

//                 case "soy":
//                     EnableLayer(soyLayer, soySprite);
//                     break;

//                 case "chips":
//                     EnableLayer(chipsLayer, chipsSprite);
//                     break;

//                 case "tahini":
//                     hasTahini = true;
//                     break;

//                 case "amba":
//                     hasAmba = true;
//                     break;
//             }
//         }

//         if (!hasPita)
//         {
//             ClearAllPitaVisuals();
//             return;
//         }

//         if (frontPita != null)
//         {
//             frontPita.gameObject.SetActive(true);
//             frontPita.enabled = true;
//         }

//         UpdateFrontPita(hasTahini, hasAmba);
//     }

//     /// <summary>
//     /// Clears every visual layer inside the pita object.
//     /// </summary>
//     public void ClearPita()
//     {
//         ClearAllPitaVisuals();
//     }

//     private void CacheAllPitaRenderers()
//     {
//         allPitaRenderers.Clear();

//         SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

//         foreach (SpriteRenderer renderer in renderers)
//         {
//             if (renderer != null && !allPitaRenderers.Contains(renderer))
//                 allPitaRenderers.Add(renderer);
//         }
//     }

//     private void ClearAllPitaVisuals()
//     {
//         if (allPitaRenderers.Count == 0)
//             CacheAllPitaRenderers();

//         foreach (SpriteRenderer renderer in allPitaRenderers)
//         {
//             if (renderer == null)
//                 continue;

//             renderer.enabled = false;
//         }

//         if (frontPita != null)
//             frontPita.gameObject.SetActive(false);
//     }

//     private void EnableLayer(SpriteRenderer renderer, Sprite sprite)
//     {
//         if (renderer == null || sprite == null)
//             return;

//         renderer.gameObject.SetActive(true);
//         renderer.sprite = sprite;
//         renderer.enabled = true;
//     }

//     private void UpdateFrontPita(bool hasTahini, bool hasAmba)
//     {
//         if (frontPita == null)
//             return;

//         if (!hasTahini && !hasAmba)
//             frontPita.sprite = frontDefault;
//         else if (hasTahini && !hasAmba)
//             frontPita.sprite = frontTahini;
//         else if (!hasTahini && hasAmba)
//             frontPita.sprite = frontAmba;
//         else
//             frontPita.sprite = frontTahiniAndAmba;
//     }
// }

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds and updates the visual representation of the pita
/// based on the currently selected ingredients.
/// </summary>
public class PitaBuilder : MonoBehaviour
{
    [Header("Pita Sprites")]
    [SerializeField] private SpriteRenderer frontPita;

    [Header("Front pita variants (sauces)")]
    [SerializeField] private Sprite frontDefault;
    [SerializeField] private Sprite frontTahini;
    [SerializeField] private Sprite frontAmba;
    [SerializeField] private Sprite frontTahiniAndAmba;

    [Header("Inner ingredient layers")]
    [SerializeField] private SpriteRenderer eggplantLayer;
    [SerializeField] private SpriteRenderer eggLayer;
    [SerializeField] private SpriteRenderer saladLayer;
    [SerializeField] private SpriteRenderer chipsLayer;
    [SerializeField] private SpriteRenderer soyLayer;

    [Header("Inner ingredient sprites")]
    [SerializeField] private Sprite eggplantSprite;
    [SerializeField] private Sprite eggSprite;
    [SerializeField] private Sprite saladSprite;
    [SerializeField] private Sprite chipsSprite;
    [SerializeField] private Sprite soySprite;

    private readonly List<SpriteRenderer> allPitaRenderers = new List<SpriteRenderer>();

    private void Awake()
    {
        CacheAllPitaRenderers();
    }

    private void Start()
    {
        ClearPita();
    }

    /// <summary>
    /// Rebuilds the pita visuals according to the selected ingredients list.
    /// Called every time the selection changes.
    /// </summary>
    public void BuildFromSelection(List<string> ingredients)
    {
        ClearPita();

        if (ingredients == null || ingredients.Count == 0)
            return;

        bool hasPita = false;
        bool hasTahini = false;
        bool hasAmba = false;

        foreach (string rawIngredient in ingredients)
        {
            if (string.IsNullOrWhiteSpace(rawIngredient))
                continue;

            string ingredient = rawIngredient.Trim().ToLowerInvariant();

            if (ingredient == "pitta")
            {
                hasPita = true;
                continue;
            }

            switch (ingredient)
            {
                case "eggplant":
                    EnableLayer(eggplantLayer, eggplantSprite);
                    break;

                case "egg":
                    EnableLayer(eggLayer, eggSprite);
                    break;

                case "salad":
                    EnableLayer(saladLayer, saladSprite);
                    break;

                case "chips":
                    EnableLayer(chipsLayer, chipsSprite);
                    break;

                case "soy":
                    EnableLayer(soyLayer, soySprite);
                    break;

                case "tahini":
                    hasTahini = true;
                    break;

                case "amba":
                    hasAmba = true;
                    break;
            }
        }

        if (!hasPita)
        {
            ClearPita();
            return;
        }

        if (frontPita != null)
        {
            frontPita.gameObject.SetActive(true);
            frontPita.enabled = true;
        }

        UpdateFrontPita(hasTahini, hasAmba);
    }

    /// <summary>
    /// Clears the pita completely, including the front pita and all ingredient layers.
    /// This is used when the pita is thrown away or when control panel settings change.
    /// </summary>
    public void ClearPita()
    {
        if (allPitaRenderers.Count == 0)
            CacheAllPitaRenderers();

        foreach (SpriteRenderer spriteRenderer in allPitaRenderers)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.enabled = false;
        }

        if (frontPita != null)
            frontPita.gameObject.SetActive(false);
    }

    private void CacheAllPitaRenderers()
    {
        allPitaRenderers.Clear();

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && !allPitaRenderers.Contains(renderer))
                allPitaRenderers.Add(renderer);
        }
    }

    private void EnableLayer(SpriteRenderer renderer, Sprite sprite)
    {
        if (renderer == null || sprite == null)
            return;

        renderer.gameObject.SetActive(true);
        renderer.sprite = sprite;
        renderer.enabled = true;
    }

    private void UpdateFrontPita(bool hasTahini, bool hasAmba)
    {
        if (frontPita == null)
            return;

        if (!hasTahini && !hasAmba)
            frontPita.sprite = frontDefault;
        else if (hasTahini && !hasAmba)
            frontPita.sprite = frontTahini;
        else if (!hasTahini && hasAmba)
            frontPita.sprite = frontAmba;
        else
            frontPita.sprite = frontTahiniAndAmba;
    }
}