using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class SessionDataCollector
{
    // Keyed by levelNumber — last attempt wins if player retries a level
    static readonly Dictionary<int, LevelAttempt> attempts = new Dictionary<int, LevelAttempt>();
    static string currentSessionId;
    static int nextPlayOrder = 0;

    // Called from EndLevel() in each level's timer script.
    // timeToTargetSeconds is passed directly from the timer.
    // levelDurationSeconds is the actual duration used (after +30/-30 adjustments).
    // originalDurationSeconds is the Inspector default before any adjustments.
    public static void RecordLevelAttempt(int levelNumber, int timeToTargetSeconds, int levelDurationSeconds, int originalDurationSeconds)
    {
        bool passed;
        int totalServed, perfectServed, duplicateClicks, glutenAppeared, glutenServed, coins, customersArrived;

        if (levelNumber == 1)
        {
            passed = LevelOneState.IsSuccess;
            totalServed = LevelOneState.TotalServedDishes;
            perfectServed = LevelOneState.PerfectServedDishes;
            duplicateClicks = LevelOneState.DuplicateIngredientClicks;
            glutenAppeared = LevelOneState.GlutenChildAppeared;
            glutenServed = LevelOneState.GlutenChildServed;
            customersArrived = LevelOneState.CustomersArrived;
        }
        else if (levelNumber == 2)
        {
            passed = LevelTwoState.IsSuccess;
            totalServed = LevelTwoState.TotalServedDishes;
            perfectServed = LevelTwoState.PerfectServedDishes;
            duplicateClicks = LevelTwoState.DuplicateIngredientClicks;
            glutenAppeared = LevelTwoState.GlutenChildAppeared;
            glutenServed = LevelTwoState.GlutenChildServed;
            customersArrived = LevelTwoState.CustomersArrived;
        }
        else if (levelNumber == 11)
        {
            passed = LevelOneOneState.IsSuccess;
            totalServed = LevelOneOneState.TotalServedDishes;
            perfectServed = LevelOneOneState.PerfectServedDishes;
            duplicateClicks = LevelOneOneState.DuplicateIngredientClicks;
            glutenAppeared = LevelOneOneState.GlutenChildAppeared;
            glutenServed = LevelOneOneState.GlutenChildServed;
            customersArrived = LevelOneOneState.CustomersArrived;
        }
        else if (levelNumber == 12)
        {
            passed = LevelOneTwoState.IsSuccess;
            totalServed = LevelOneTwoState.TotalServedDishes;
            perfectServed = LevelOneTwoState.PerfectServedDishes;
            duplicateClicks = LevelOneTwoState.DuplicateIngredientClicks;
            glutenAppeared = LevelOneTwoState.GlutenChildAppeared;
            glutenServed = LevelOneTwoState.GlutenChildServed;
            customersArrived = LevelOneTwoState.CustomersArrived;
        }
        else if (levelNumber == 21)
        {
            passed = LevelTwoOneState.IsSuccess;
            totalServed = LevelTwoOneState.TotalServedDishes;
            perfectServed = LevelTwoOneState.PerfectServedDishes;
            duplicateClicks = LevelTwoOneState.DuplicateIngredientClicks;
            glutenAppeared = LevelTwoOneState.GlutenChildAppeared;
            glutenServed = LevelTwoOneState.GlutenChildServed;
            customersArrived = LevelTwoOneState.CustomersArrived;
        }
        else if (levelNumber == 22)
        {
            passed = LevelTwoTwoState.IsSuccess;
            totalServed = LevelTwoTwoState.TotalServedDishes;
            perfectServed = LevelTwoTwoState.PerfectServedDishes;
            duplicateClicks = LevelTwoTwoState.DuplicateIngredientClicks;
            glutenAppeared = LevelTwoTwoState.GlutenChildAppeared;
            glutenServed = LevelTwoTwoState.GlutenChildServed;
            customersArrived = LevelTwoTwoState.CustomersArrived;
        }
        else // level 3
        {
            passed = LevelThreeState.IsSuccess;
            totalServed = LevelThreeState.TotalServedDishes;
            perfectServed = LevelThreeState.PerfectServedDishes;
            duplicateClicks = LevelThreeState.DuplicateIngredientClicks;
            glutenAppeared = LevelThreeState.GlutenChildAppeared;
            glutenServed = LevelThreeState.GlutenChildServed;
            customersArrived = LevelThreeState.CustomersArrived;
        }

        coins = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentMoney : 0;

        // Derived metrics — no new runtime tracking needed
        int incorrectDishes = totalServed - perfectServed;
        int glutenHandled = glutenAppeared - glutenServed;
        float avgPrepTime = perfectServed > 0
            ? (float)timeToTargetSeconds / perfectServed
            : -1f;

        // Collect control panel settings from live managers at the moment EndLevel fires
        var controlSettings = new LevelControlSettings
        {
            levelDurationSeconds = levelDurationSeconds,
            levelDurationDefault = originalDurationSeconds,
            angerTimeSeconds = Mathf.RoundToInt(CustomerMoodTimer_levels.RuntimeSecondsPerStage),
            markAddedItemsEnabled = ControlPanelUI.MarkAddedItemsEnabled,
            dirtEnabled = ControlPanelUI.DirtEnabled,
            concurrentCustomers = CustomerManager.Instance != null ? CustomerManager.Instance.GetCurrentMaxConcurrentCustomers() : 1,
            concurrentCustomersMax = CustomerManager.Instance != null ? CustomerManager.Instance.GetMaxSupportedConcurrentCustomers() : 1,
            ingredientCount = LevelIngredientAvailabilityManager.Instance != null ? LevelIngredientAvailabilityManager.Instance.CurrentIngredientCount : 0,
            ingredientCountMax = LevelIngredientAvailabilityManager.Instance != null ? LevelIngredientAvailabilityManager.Instance.MaxIngredientCount : 0,
        };

        attempts[levelNumber] = new LevelAttempt
        {
            levelNumber = levelNumber,
            attempted = true,
            playOrder = ++nextPlayOrder,
            passed = passed,
            coins = coins,
            timeToTargetSeconds = timeToTargetSeconds,
            totalServedDishes = totalServed,
            perfectServedDishes = perfectServed,
            incorrectDishes = incorrectDishes,
            duplicateIngredientClicks = duplicateClicks,
            glutenChildAppeared = glutenAppeared,
            glutenChildServedByMistake = glutenServed,
            glutenChildHandledCorrectly = glutenHandled,
            averageDishPrepTimeSeconds = avgPrepTime,
            customersArrived = customersArrived,
            controlSettings = controlSettings,
        };

        Debug.Log(
            $"[SDC] RecordLevelAttempt: level={levelNumber} passed={passed} coins={coins} "
                + $"timeToTarget={timeToTargetSeconds} totalServed={totalServed} perfectServed={perfectServed} "
                + $"attempts.Count={attempts.Count}"
        );
    }

    // Called when the player returns to MainMenu (via CloudProgressTracker).
    public static async Task FinalizeAndExport()
    {
        Debug.Log($"[SDC] FinalizeAndExport called: currentSessionId={currentSessionId} attempts.Count={attempts.Count}");

        // No gameplay yet (e.g. MainMenu loaded immediately after login) — nothing to export.
        if (attempts.Count == 0)
        {
            Debug.Log("[SDC] FinalizeAndExport: returning early — attempts empty (no gameplay yet)");
            return;
        }

        // Generate session ID once; reuse it on subsequent end-scene exports within the same play-through.
        if (currentSessionId == null)
            currentSessionId = SessionIdentity.GenerateSessionId();

        Debug.Log($"[SDC] FinalizeAndExport: proceeding — sessionId={currentSessionId} levels to export={attempts.Count}");

        var levelList = new List<LevelAttempt>(attempts.Values);

        var record = new SessionRecord
        {
            sessionId = currentSessionId,
            displayName = SessionIdentity.DisplayName ?? "Unknown",
            internalUsername = SessionIdentity.InternalUsername ?? "unknown",
            isGuest = SessionIdentity.IsGuest,
            sessionDateTimeISO = System.DateTime.UtcNow.ToString("o"),
            resumeScene = "",
            levels = levelList,
        };

        await CloudSessionHistory.AppendSession(record);
    }

    // Called at session start (login) to clear state from any previous session
    public static void Reset()
    {
        attempts.Clear();
        currentSessionId = null;
        nextPlayOrder = 0;
    }
}