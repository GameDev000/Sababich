using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// Sends session/level data to the external Supabase database.
// Supabase serves as a central store for ALL players —
// the external researcher dashboard reads from it.
//
// Unity Cloud Save  → per-player data (game progress, resume scene)
// Supabase          → all players aggregated (researcher dashboard)
public static class SupabaseClient
{
    // ── Supabase connection ───────────────────────────────────────

    private const string ProjectUrl = "https://pojdoefmxxdlxhoxjbvk.supabase.co";

    // Publishable (anon) key — safe to be in game code.
    // Write permission is granted via the RLS policy "allow insert from game".
    // Read permission is blocked for this key — only the secret key (in the dashboard) can read.
    private const string AnonKey = "sb_publishable_UwRoPAdg_hCqjM7o43-x0w_tXYEXmi0";

    // on_conflict tells Supabase: if a row with the same (session_id, level_number) exists,
    // UPDATE it instead of failing. This makes every call idempotent —
    // safe to repeat when FinalizeAndExport fires multiple times per session.
    private const string Endpoint = "/rest/v1/sessions?on_conflict=session_id,level_number";

    // ── Row shape — must match Supabase table columns exactly ─────

    [System.Serializable]
    private class SessionRow
    {
        public string session_id;
        public string display_name;
        public string session_date;
        public int    level_number;
        public bool   passed;
        public int    coins;
        public int    time_seconds;
        public int    total_served;
        public int    customers_arrived;
        public int    perfect_served;
        public int    duplicate_clicks;
        public int    gluten_appeared;
        public int    gluten_served;
    }

    // ── Public API ────────────────────────────────────────────────

    // Upserts all level attempts from a session into Supabase.
    // Called from CloudSessionHistory after every session export.
    // Guests are skipped — their data stays local only.
    // Safe to call multiple times for the same session (upsert, not insert).
    public static async Task UpsertSession(SessionRecord record)
    {
        if (record.isGuest) return;
        if (record.levels == null || record.levels.Count == 0) return;

        // One HTTP request per level attempt (Supabase stores one row per level)
        foreach (var level in record.levels)
            await UpsertRow(record, level);
    }

    // ── Private ───────────────────────────────────────────────────

    // Builds a single row and sends it to Supabase via HTTP POST.
    // If a row with the same (session_id, level_number) already exists it is updated.
    private static async Task UpsertRow(SessionRecord record, LevelAttempt level)
    {
        var row = new SessionRow
        {
            session_id        = record.sessionId,
            display_name      = record.displayName,
            session_date      = record.sessionDateTimeISO,
            level_number      = level.levelNumber,
            passed            = level.passed,
            coins             = level.coins,
            time_seconds      = level.timeToTargetSeconds,
            total_served      = level.totalServedDishes,
            customers_arrived = level.customersArrived,
            perfect_served    = level.perfectServedDishes,
            duplicate_clicks  = level.duplicateIngredientClicks,
            gluten_appeared   = level.glutenChildAppeared,
            gluten_served     = level.glutenChildServedByMistake,
        };

        string json = JsonUtility.ToJson(row);
        string url  = ProjectUrl + Endpoint;

        using var request = new UnityWebRequest(url, "POST");
        request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();

        // Required headers for Supabase REST API
        request.SetRequestHeader("Content-Type",  "application/json");
        request.SetRequestHeader("apikey",        AnonKey);
        request.SetRequestHeader("Authorization", "Bearer " + AnonKey);
        // merge-duplicates = upsert: update the existing row on conflict, insert if new
        request.SetRequestHeader("Prefer", "resolution=merge-duplicates,return=minimal");

        var op = request.SendWebRequest();

        // Await without blocking the main thread
        while (!op.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log($"[Supabase] Upserted level {level.levelNumber} for '{record.displayName}'");
        else
            Debug.LogError($"[Supabase] Upsert failed (level {level.levelNumber}): {request.error} — {request.downloadHandler.text}");
    }
}
