using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomerType
{
    public string name;
    public Sprite sprite;
    public List<string> requiredIngredients;

    [Header("Mood Sprites")]
    public Sprite happyFace;
    public Sprite[] angryFaces;
}
