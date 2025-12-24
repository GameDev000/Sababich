using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class SelectionListTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void SelectionListSimplePasses()
    {
        var obj1 = new GameObject("Sel1");
        var selectionList1 = obj1.AddComponent<SelectionList>();

        var obj2 = new GameObject("Sel2");
        var selectionList2 = obj2.AddComponent<SelectionList>();

        Assert.AreNotEqual(selectionList1, selectionList2); //check that two different instances are not created

    }

    List<string> correctOrder = new List<string> { "pitta", "thini", "salad" };
    [Test]
    public void IsSelectionMatchingTestMatch()
    {
        List<string> selectedIngredients = new List<string> { "pitta", "thini", "salad" };
        var obj1 = new GameObject("Sel1");
        var selectionList1 = obj1.AddComponent<SelectionList>();
        foreach (var ingredient in selectedIngredients)
        {
            selectionList1.TryAddIngredient(ingredient);
        }
        Assert.IsTrue(selectionList1.IsSelectionMatching(correctOrder)); //check that the selection matches the correct order

    }

    [Test]
    public void IsSelectionMatchingTestNotMatch()
    {
        List<string> selectedIngredients = new List<string> { "pitta", "thini", "amba" };
        var obj1 = new GameObject("Sel1");
        var selectionList1 = obj1.AddComponent<SelectionList>();
        foreach (var ingredient in selectedIngredients)
        {
            selectionList1.TryAddIngredient(ingredient);
        }
        Assert.IsFalse(selectionList1.IsSelectionMatching(correctOrder)); //check that the selection does not match the correct order

    }

    [Test]
    public void FirstIngredient_NotPitta_ReturnsFalse()
    {
        var obj1 = new GameObject("Sel1");
        var selectionList1 = obj1.AddComponent<SelectionList>();

        bool result = selectionList1.TryAddIngredient("Tomato");

        Assert.IsFalse(result); //check that adding an ingredient other than "Pitta" first returns false

    }
    [Test]
    public void FirstIngredient_NotPitta_ReturnsTrue()
    {
        var obj1 = new GameObject("Sel1");
        var selectionList1 = obj1.AddComponent<SelectionList>();

        bool result = selectionList1.TryAddIngredient("Pitta");

        Assert.IsTrue(result); //check that adding "Pitta" first returns true

    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var obj in Object.FindObjectsOfType<GameObject>())
            Object.DestroyImmediate(obj);
    }

}
