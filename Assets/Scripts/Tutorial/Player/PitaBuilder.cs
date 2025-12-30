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
    // Different visual variants depending on sauce combinations
    [SerializeField] private Sprite frontDefault;
    [SerializeField] private Sprite frontTahini;
    [SerializeField] private Sprite frontAmba;
    [SerializeField] private Sprite frontTahiniAndAmba;

    [Header("Inner ingredient layers")]
    // Sprite layers for each possible inner ingredient
    [SerializeField] private SpriteRenderer eggplantLayer;
    [SerializeField] private SpriteRenderer eggLayer;
    [SerializeField] private SpriteRenderer saladLayer;
    [SerializeField] private SpriteRenderer chipsLayer;
    [SerializeField] private SpriteRenderer soyLayer;

    [Header("Inner ingredient sprites")]
    // Sprites assigned to each ingredient layer
    [SerializeField] private Sprite eggplantSprite;
    [SerializeField] private Sprite eggSprite;
    [SerializeField] private Sprite saladSprite;
    [SerializeField] private Sprite chipsSprite;
    [SerializeField] private Sprite soySprite;

    private void Start()
    {
        // At game start, the pita should not be visible until the player selects "pitta"
        if (frontPita != null)
        {
            frontPita.gameObject.SetActive(false);
        }

        // Ensure no inner ingredients are visible at start
        ClearInnerLayers();
    }

    /// <summary>
    /// Rebuilds the pita visuals according to the selected ingredients list.
    /// Called every time the selection changes.
    /// </summary>
    public void BuildFromSelection(List<string> ingredients)
    {
        // Reset all ingredient layers before rebuilding
        ClearInnerLayers();

        bool hasTahini = false;
        bool hasAmba = false;
        bool hasPita = false;

        foreach (var rawIng in ingredients)
        {
            string ing = rawIng.ToLower();

            if (ing == "pitta")
            {
                hasPita = true;
                continue;
            }

            switch (ing)
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

                case "soy":
                    EnableLayer(soyLayer, soySprite);
                    break;

                case "chips":
                    EnableLayer(chipsLayer, chipsSprite);
                    break;

                // Sauces affect
                case "tahini":
                    hasTahini = true;
                    break;

                case "amba":
                    hasAmba = true;
                    break;
            }
        }

        // If no pita was selected, hide everything and stop
        if (!hasPita)
        {
            if (frontPita != null)
                frontPita.gameObject.SetActive(false);

            ClearInnerLayers();
            return;
        }

        // Pita exists show front sprite and update
        if (frontPita != null)
            frontPita.gameObject.SetActive(true);

        UpdateFrontPita(hasTahini, hasAmba);
    }

    private void ClearInnerLayers()
    {
        if (eggplantLayer != null) eggplantLayer.enabled = false;
        if (eggLayer != null) eggLayer.enabled = false;
        if (saladLayer != null) saladLayer.enabled = false;
        if (chipsLayer != null) chipsLayer.enabled = false;
        if (soyLayer != null) soyLayer.enabled = false;
    }

    /// Enables a specific ingredient layer and assigns its sprite.
    private void EnableLayer(SpriteRenderer renderer, Sprite sprite)
    {
        if (renderer == null || sprite == null) return;

        renderer.sprite = sprite;
        renderer.enabled = true;
    }

    /// Chooses the correct front pita sprite based on sauce combinations.
    private void UpdateFrontPita(bool hasTahini, bool hasAmba)
    {
        if (frontPita == null) return;

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
