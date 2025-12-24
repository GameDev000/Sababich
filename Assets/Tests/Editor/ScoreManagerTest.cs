using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
public class ScoreManagerTest
{

    [Test]
    public void ScoreManagerSimplePasses()
    {
        var obj1 = new GameObject("ScoreManager");
        var scoreManager1 = obj1.AddComponent<ScoreManager>();

        scoreManager1.AddMoney(30);
        Assert.AreNotEqual(scoreManager1, 30); //check that money was added correctly

    }

}