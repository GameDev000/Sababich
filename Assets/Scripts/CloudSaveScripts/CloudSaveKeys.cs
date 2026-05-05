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
    public const string Level1Passed = "level1_passed";
    public const string Level1Coins = "level1_coins";
    public const string Level1TimeSeconds = "level1_timeSeconds";
    public const string Level1TotalServed = "level1_totalServed";
    public const string Level1PerfectServed = "level1_perfectServed";

    // ── Level 2 ──────────────────────────────────────────────────
    public const string Level2Passed = "level2_passed";
    public const string Level2Coins = "level2_coins";
    public const string Level2TimeSeconds = "level2_timeSeconds";
    public const string Level2TotalServed = "level2_totalServed";
    public const string Level2PerfectServed = "level2_perfectServed";

    // ── Level 3 ──────────────────────────────────────────────────
    public const string Level3Passed = "level3_passed";
    public const string Level3Coins = "level3_coins";
    public const string Level3TimeSeconds = "level3_timeSeconds";
    public const string Level3TotalServed = "level3_totalServed";
    public const string Level3PerfectServed = "level3_perfectServed";

    // ── Dynamic key helpers (used by EndOfLevelUI) ───────────────
    public static string CoinsKey(int level) => $"level{level}_coins";
    public static string TotalServedKey(int level) => $"level{level}_totalServed";
    public static string PerfectServedKey(int level) => $"level{level}_perfectServed";
    public static string DuplicateClicksKey(int level) => $"level{level}_duplicateClicks";
    public static string GlutenChildAppearedKey(int level) => $"level{level}_glutenChildAppeared";
    public static string GlutenChildServedKey(int level) => $"level{level}_glutenChildServed";

    // ── Dashboard session history ─────────────────────────────────
    public const string DashboardSessionCount = "dashboard_session_count";
    public static string DashboardSessionKey(int n) => $"dashboard_session_{n}";
}
