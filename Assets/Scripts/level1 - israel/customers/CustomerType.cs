using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Defines the data structure for a customer type, including their appearance and order requirements.
/// </summary>
[System.Serializable]
public class CustomerType
{
    public string name;
    public Sprite sprite;
    public List<string> requiredIngredients;

    [Header("Mood Sprites")]
    public Sprite happyFace;
    public Sprite[] angryFaces;
    [Header("Special Rules")]
    public bool scoreIfNotServed = false;

}
