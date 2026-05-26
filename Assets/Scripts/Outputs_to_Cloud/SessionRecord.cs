using System.Collections.Generic;

[System.Serializable]
public class LevelControlSettings
{
    public int levelDurationSeconds;     // actual duration used
    public int levelDurationDefault;     // original Inspector value
    public int angerTimeSeconds;         // patience meter (7–12)
    public bool markAddedItemsEnabled;   // ingredient marking toggle
    public bool dirtEnabled;             // dirt toggle
    public int concurrentCustomers;      // actual customer limit
    public int concurrentCustomersMax;   // max supported for this level (= default)
    public int ingredientCount;          // actual ingredient count
    public int ingredientCountMax;       // max for this level (= default)
}

[System.Serializable]
public class LevelAttempt
{
    public int levelNumber;
    public bool attempted;
    public bool passed;
    public int coins;
    public int timeToTargetSeconds;        // -1 if coin target was never reached
    public int totalServedDishes;
    public int perfectServedDishes;
    public int incorrectDishes;            // derived: totalServed - perfectServed
    public int duplicateIngredientClicks;
    public int glutenChildAppeared;
    public int glutenChildServedByMistake;
    public int glutenChildHandledCorrectly; // derived: appeared - servedByMistake
    public float averageDishPrepTimeSeconds; // derived: timeToTarget / perfectServed, -1 if N/A
    public int customersArrived;
    public int playOrder;                        // 1-based order in which this level was played
    public LevelControlSettings controlSettings;
}

[System.Serializable]
public class SessionRecord
{
    public string sessionId;
    public string displayName;
    public string internalUsername;
    public bool isGuest;
    public string sessionDateTimeISO;
    public string resumeScene;
    public List<LevelAttempt> levels;
}
