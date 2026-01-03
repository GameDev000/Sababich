using System.Collections.Generic;
using UnityEngine;

public static class RuntimeCustomerFactory
{
    public static CustomerType CreateFromBase(
        CustomerType baseType,
        Sprite happy,
        Sprite angry,
        Sprite furious,
        string newName = "PlayerCustomer"
    )
    {
        if (baseType == null) return null;

        var t = new CustomerType();
        t.name = newName;

        // Body stays constant (base customer's body)
        t.sprite = baseType.sprite;

        // Copy requirements & rules
        t.requiredIngredients = baseType.requiredIngredients != null
            ? new List<string>(baseType.requiredIngredients)
            : new List<string>();

        t.scoreIfNotServed = baseType.scoreIfNotServed;

        // Replace ONLY faces
        t.happyFace = happy != null ? happy : baseType.happyFace;

        // Ensure 2 angry faces (angry, furious)
        t.angryFaces = new Sprite[2];
        t.angryFaces[0] = angry != null ? angry : (baseType.angryFaces != null && baseType.angryFaces.Length > 0 ? baseType.angryFaces[0] : null);
        t.angryFaces[1] = furious != null ? furious : (baseType.angryFaces != null && baseType.angryFaces.Length > 1 ? baseType.angryFaces[1] : null);

        return t;
    }
}
