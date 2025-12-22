using System.Collections.Generic;
using UnityEngine;

public class PitaBuilder : MonoBehaviour
{
    [Header("Pita Sprites")]
    [SerializeField] private SpriteRenderer backPita;
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

    private void Start()
    {
        if (frontPita != null)
        {
            frontPita.gameObject.SetActive(false);
        }

        ClearInnerLayers();
    }

    public void BuildFromSelection(List<string> ingredients)
    {
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
                case "חציל":
                    EnableLayer(eggplantLayer, eggplantSprite);
                    break;

                case "egg":
                case "ביצה":
                    EnableLayer(eggLayer, eggSprite);
                    break;

                case "salad":
                case "סלט":
                    EnableLayer(saladLayer, saladSprite);
                    break;

                case "soy":
                case "סויה":
                    EnableLayer(soyLayer, soySprite);
                    break;    

                case "chips":
                case "ציפס":
                    EnableLayer(chipsLayer, chipsSprite);
                    break;

                case "tahini":
                case "טחינה":
                    hasTahini = true;
                    break;

                case "amba":
                case "עמבה":
                    hasAmba = true;
                    break;
            }
        }

        if (!hasPita)
        {
            if (frontPita != null)
                frontPita.gameObject.SetActive(false);

            ClearInnerLayers();
            return;
        }

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

    private void EnableLayer(SpriteRenderer renderer, Sprite sprite)
    {
        if (renderer == null || sprite == null) return;

        renderer.sprite = sprite;
        renderer.enabled = true;
    }

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
