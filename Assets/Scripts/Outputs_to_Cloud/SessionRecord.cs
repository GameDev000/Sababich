using System.Collections.Generic;

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
    public int lastLevelReached;
    public List<LevelAttempt> levels;
}

// JsonUtility requires a class wrapper — cannot serialize a bare List<T> at the root.
[System.Serializable]
public class SessionDataFile
{
    public List<SessionRecord> sessions;
}
