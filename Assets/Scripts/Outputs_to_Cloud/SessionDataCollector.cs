using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class SessionDataCollector
{
    // Keyed by levelNumber — last attempt wins if player retries a level
    static readonly Dictionary<int, LevelAttempt> attempts = new Dictionary<int, LevelAttempt>();
    static int lastLevelReached;
    static bool exportCalled;

    // Called from EndLevel() in each level's timer script.
    // timeToTargetSeconds is passed directly from the timer (-1 if target was never reached).
    public static void RecordLevelAttempt(int levelNumber, int timeToTargetSeconds)
    {
        bool passed;
        int totalServed, perfectServed, duplicateClicks, glutenAppeared, glutenServed, coins;

        if (levelNumber == 1)
        {
            passed          = LevelOneState.IsSuccess;
            totalServed     = LevelOneState.TotalServedDishes;
            perfectServed   = LevelOneState.PerfectServedDishes;
            duplicateClicks = LevelOneState.DuplicateIngredientClicks;
            glutenAppeared  = LevelOneState.GlutenChildAppeared;
            glutenServed    = LevelOneState.GlutenChildServed;
        }
        else if (levelNumber == 2)
        {
            passed          = LevelTwoState.IsSuccess;
            totalServed     = LevelTwoState.TotalServedDishes;
            perfectServed   = LevelTwoState.PerfectServedDishes;
            duplicateClicks = LevelTwoState.DuplicateIngredientClicks;
            glutenAppeared  = LevelTwoState.GlutenChildAppeared;
            glutenServed    = LevelTwoState.GlutenChildServed;
        }
        else // level 3
        {
            passed          = LevelThreeState.IsSuccess;
            totalServed     = LevelThreeState.TotalServedDishes;
            perfectServed   = LevelThreeState.PerfectServedDishes;
            duplicateClicks = LevelThreeState.DuplicateIngredientClicks;
            glutenAppeared  = LevelThreeState.GlutenChildAppeared;
            glutenServed    = LevelThreeState.GlutenChildServed;
        }

        coins = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentMoney : 0;

        // Derived metrics — no new runtime tracking needed
        int incorrectDishes = totalServed - perfectServed;
        int glutenHandled   = glutenAppeared - glutenServed;
        float avgPrepTime   = (perfectServed > 0 && timeToTargetSeconds != -1)
                              ? (float)timeToTargetSeconds / perfectServed
                              : -1f;

        attempts[levelNumber] = new LevelAttempt
        {
            levelNumber                 = levelNumber,
            attempted                   = true,
            passed                      = passed,
            coins                       = coins,
            timeToTargetSeconds         = timeToTargetSeconds,
            totalServedDishes           = totalServed,
            perfectServedDishes         = perfectServed,
            incorrectDishes             = incorrectDishes,
            duplicateIngredientClicks   = duplicateClicks,
            glutenChildAppeared         = glutenAppeared,
            glutenChildServedByMistake  = glutenServed,
            glutenChildHandledCorrectly = glutenHandled,
            averageDishPrepTimeSeconds  = avgPrepTime,
        };

        if (levelNumber > lastLevelReached)
            lastLevelReached = levelNumber;

        Debug.Log($"[SDC] RecordLevelAttempt: level={levelNumber} passed={passed} coins={coins} " +
                  $"timeToTarget={timeToTargetSeconds} totalServed={totalServed} perfectServed={perfectServed} " +
                  $"attempts.Count={attempts.Count}");
    }

    // Called when the player returns to MainMenu (via CloudProgressTracker).
    public static async Task FinalizeAndExport()
    {
        Debug.Log($"[SDC] FinalizeAndExport called: exportCalled={exportCalled} attempts.Count={attempts.Count}");

        // Guard against double export (CloudProgressTracker fires on every scene load)
        if (exportCalled) { Debug.Log("[SDC] FinalizeAndExport: returning early — exportCalled=true"); return; }

        // No gameplay yet (e.g. MainMenu loaded immediately after login) — don't lock the guard.
        if (attempts.Count == 0) { Debug.Log("[SDC] FinalizeAndExport: returning early — attempts empty (no gameplay yet)"); return; }

        exportCalled = true;
        Debug.Log($"[SDC] FinalizeAndExport: proceeding — exportCalled locked, levels to export={attempts.Count}");

        var levelList = new List<LevelAttempt>(attempts.Values);

        var record = new SessionRecord
        {
            sessionId          = SessionIdentity.GenerateSessionId(),
            displayName        = SessionIdentity.DisplayName ?? "Unknown",
            internalUsername   = SessionIdentity.InternalUsername ?? "unknown",
            isGuest            = SessionIdentity.IsGuest,
            sessionDateTimeISO = System.DateTime.UtcNow.ToString("o"),
            resumeScene        = "",   // SessionExporter fills this from CloudProgressTracker if needed
            lastLevelReached   = lastLevelReached,
            levels             = levelList,
        };

        await SessionExporter.ExportSession(record);
    }

    // Called at session start (login) to clear state from any previous session
    public static void Reset()
    {
        attempts.Clear();
        lastLevelReached = 0;
        exportCalled = false;
    }
}
