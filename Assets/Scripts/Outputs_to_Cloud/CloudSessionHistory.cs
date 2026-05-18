using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public static class CloudSessionHistory
{
    // Maximum sessions stored in cloud before the ring buffer wraps
    const int MaxSessions = 200;

    // Appends a new session to cloud storage.
    // Skipped entirely for guest users — no cloud write.
    public static async Task AppendSession(SessionRecord record)
    {
        if (record.isGuest) return;

        if (UnityServices.State != ServicesInitializationState.Initialized) return;
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var countData = await DatabaseManager.LoadData(CloudSaveKeys.DashboardSessionCount);
        int count = DatabaseManager.ReadInt(countData, CloudSaveKeys.DashboardSessionCount, 0);

        string json = JsonUtility.ToJson(record);

        // If the most recent slot holds the same session, overwrite it without incrementing count.
        if (count > 0)
        {
            string lastKey = CloudSaveKeys.DashboardSessionKey((count - 1) % MaxSessions);
            var lastData = await DatabaseManager.LoadData(lastKey);
            string lastJson = DatabaseManager.ReadString(lastData, lastKey, "");
            if (!string.IsNullOrEmpty(lastJson))
            {
                try
                {
                    var lastRecord = JsonUtility.FromJson<SessionRecord>(lastJson);
                    if (lastRecord?.sessionId == record.sessionId)
                    {
                        await DatabaseManager.SaveData((lastKey, json));
                        Debug.Log($"[CloudSessionHistory] Updated existing session slot {(count - 1) % MaxSessions} (same sessionId)");
                        // Also upsert to Supabase — handles new levels added after first export
                        await SupabaseClient.UpsertSession(record);
                        return;
                    }
                }
                catch { }
            }
        }

        // New session — use next slot and increment count.
        string sessionKey = CloudSaveKeys.DashboardSessionKey(count % MaxSessions);
        await DatabaseManager.SaveData(
            (sessionKey, json),
            (CloudSaveKeys.DashboardSessionCount, count + 1)
        );
        Debug.Log($"[CloudSessionHistory] Saved new session slot {count % MaxSessions} (total={count + 1})");
        // Send to Supabase so the researcher dashboard receives this session
        await SupabaseClient.UpsertSession(record);
    }

}
