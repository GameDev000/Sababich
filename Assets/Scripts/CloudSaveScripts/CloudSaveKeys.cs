/// <summary>
/// Single source of truth for all Unity Cloud Save key strings.
/// Use these constants everywhere instead of hardcoded strings.
/// </summary>
public static class CloudSaveKeys
{
    // ── Identity ────────────────────────────────────────────────
    public const string DisplayName = "displayName";
    public const string Username = "username";
    // "user_of_{hash}" is generated dynamically — see ScoreOfAuthenticatedUser.NameMapKey()

    // ── Progress ─────────────────────────────────────────────────
    public const string ResumeScene = "resumeScene";

    // ── Level 1 ──────────────────────────────────────────────────
    public const string Level1Passed = "1_passed";
    public const string Level1Coins = "1_coins";
    public const string Level1TimeSeconds = "1_timeSeconds";
    public const string Level1TotalServed = "1_totalServed";
    public const string Level1PerfectServed = "1_perfectServed";

    // ── Level 2 ──────────────────────────────────────────────────
    public const string Level2Passed = "2_passed";
    public const string Level2Coins = "2_coins";
    public const string Level2TimeSeconds = "2_timeSeconds";
    public const string Level2TotalServed = "2_totalServed";
    public const string Level2PerfectServed = "2_perfectServed";

    // ── Level 3 ──────────────────────────────────────────────────
    public const string Level3Passed = "3_passed";
    public const string Level3Coins = "3_coins";
    public const string Level3TimeSeconds = "3_timeSeconds";
    public const string Level3TotalServed = "3_totalServed";
    public const string Level3PerfectServed = "3_perfectServed";

    // ── Level 1.1 ─────────────────────────────────────────────────
    public const string Level1_1Passed = "1_1_passed";
    public const string Level1_1Coins = "1_1_coins";
    public const string Level1_1TimeSeconds = "1_1_timeSeconds";
    public const string Level1_1TotalServed = "1_1_totalServed";
    public const string Level1_1PerfectServed = "1_1_perfectServed";

    // ── Level 1.2 ─────────────────────────────────────────────────
    public const string Level1_2Passed = "1_2_passed";
    public const string Level1_2Coins = "1_2_coins";
    public const string Level1_2TimeSeconds = "1_2_timeSeconds";
    public const string Level1_2TotalServed = "1_2_totalServed";
    public const string Level1_2PerfectServed = "1_2_perfectServed";

    // ── Dynamic key helpers ───────────────────────────────────────
    public static string CoinsKey(int level) => $"{level}_coins";
    public static string TotalServedKey(int level) => $"{level}_totalServed";
    public static string PerfectServedKey(int level) => $"{level}_perfectServed";
    public static string CustomersArrivedKey(int level) => $"{level}_customersArrived";
    public static string CustomersArrivedKey(string levelId) => $"{levelId.Replace('.', '_')}_customersArrived";
    public static string DuplicateClicksKey(int level) => $"{level}_duplicateClicks";
    public static string GlutenChildAppearedKey(int level) => $"{level}_glutenChildAppeared";
    public static string GlutenChildServedKey(int level) => $"{level}_glutenChildServed";
    public static string DuplicateClicksKey(string levelId) => $"{levelId.Replace('.', '_')}_duplicateClicks";
    public static string GlutenChildAppearedKey(string levelId) => $"{levelId.Replace('.', '_')}_glutenChildAppeared";
    public static string GlutenChildServedKey(string levelId) => $"{levelId.Replace('.', '_')}_glutenChildServed";

    // ── Dashboard session history ─────────────────────────────────
    public const string DashboardSessionCount = "dashboard_session_count";
    public static string DashboardSessionKey(int n) => $"dashboard_session_{n}";
}
