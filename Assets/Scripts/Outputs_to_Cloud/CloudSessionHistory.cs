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

        // Load current session count (default 0 for new player)
        var countData = await DatabaseManager.LoadData(CloudSaveKeys.DashboardSessionCount);
        int count = DatabaseManager.ReadInt(countData, CloudSaveKeys.DashboardSessionCount, 0);

        // Serialize session to compact JSON and save to its numbered slot
        string json = JsonUtility.ToJson(record);
        string sessionKey = CloudSaveKeys.DashboardSessionKey(count % MaxSessions);

        await DatabaseManager.SaveData(
            (sessionKey, json),
            (CloudSaveKeys.DashboardSessionCount, count + 1)
        );

        Debug.Log($"[CloudSessionHistory] Saved session slot {count % MaxSessions} (total={count + 1})");
    }

    // Loads all sessions for the current player from cloud.
    public static async Task<List<SessionRecord>> LoadAllSessions()
    {
        var result = new List<SessionRecord>();

        if (UnityServices.State != ServicesInitializationState.Initialized) return result;
        if (!AuthenticationService.Instance.IsSignedIn) return result;

        var countData = await DatabaseManager.LoadData(CloudSaveKeys.DashboardSessionCount);
        int count = DatabaseManager.ReadInt(countData, CloudSaveKeys.DashboardSessionCount, 0);
        if (count == 0) return result;

        // Build key list for batch load — cap at MaxSessions (ring buffer)
        int slots = Mathf.Min(count, MaxSessions);
        var keys = new string[slots];
        for (int i = 0; i < slots; i++)
            keys[i] = CloudSaveKeys.DashboardSessionKey(i);

        var data = await DatabaseManager.LoadData(keys);

        for (int i = 0; i < slots; i++)
        {
            string json = DatabaseManager.ReadString(data, keys[i], "");
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[CloudSessionHistory] Missing session slot {i} — skipping.");
                continue;
            }

            try
            {
                result.Add(JsonUtility.FromJson<SessionRecord>(json));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CloudSessionHistory] Failed to parse slot {i}: {e.Message}");
            }
        }

        return result;
    }
}
