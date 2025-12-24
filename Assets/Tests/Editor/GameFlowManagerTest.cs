
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
public class GameFlowManagerTest
{
    public GamePhase CurrentPhase;
    // A Test behaves as an ordinary method
    [Test]
    public void SetPhaseTest()
    {
        var obj1 = new GameObject("GameFlowManager");
        var gameFlowManager = obj1.AddComponent<GameFlowManager>();


        CurrentPhase = GamePhase.AddRowEggplant;
        gameFlowManager.SetPhase(CurrentPhase);

        Assert.AreEqual(GamePhase.AddRowEggplant, gameFlowManager.CurrentPhase); //check that the phase is set to AddRowEggplant

        CurrentPhase = GamePhase.AssembleDish;
        gameFlowManager.SetPhase(CurrentPhase);

        Assert.AreEqual(GamePhase.AssembleDish, gameFlowManager.CurrentPhase); //check that the phase is set to AssembleDish

    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var obj in Object.FindObjectsOfType<GameObject>())
            Object.DestroyImmediate(obj);
    }
}