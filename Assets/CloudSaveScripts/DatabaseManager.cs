using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Authentication;   // AuthenticationService
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;             // UnityServices
using UnityEngine;
using System.Linq;

// IMPORTANT: We have OUR OWN script named "Item" in Sababich.
// Cloud Save also has a type named "Item".
// So we MUST use an alias to avoid name collisions.
using CloudSaveItem = Unity.Services.CloudSave.Models.Item;

/*
 * This class manages loading and saving key-value pairs in the CloudSaveService.
 */
public class DatabaseManager
{
    // Sample code from https://docs.unity.com/ugs/manual/cloud-save/manual/tutorials/unity-sdk

    public static async Task<Dictionary<string, string>> SaveData(params (string key, object value)[] kwargs)
    {
        // Idea from  here: https://stackoverflow.com/a/77002085/827927
        Dictionary<string, object> playerData = kwargs.ToDictionary(x => x.key, x => x.value);
        var result = await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        Debug.Log($"Saved data {string.Join(',', playerData)}. result={string.Join(',', result)}");
        return result;
    }

    public static async Task<Dictionary<string, CloudSaveItem>> LoadData(params string[] args)
    {
        Debug.Log($"LoadData {string.Join(',', args)}");
        HashSet<string> keys = new HashSet<string>(args);

        // CloudSave returns: Dictionary<string, Unity.Services.CloudSave.Models.Item>
        Dictionary<string, CloudSaveItem> playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

        Debug.Log($"loaded player data: {string.Join(',', playerData)}");
        return playerData;
    }

    /* ===================================================
     * SAFE READ HELPERS (Sababich)
     * ===================================================
     * We add these helpers so game scripts can read values without crashing.
     * They also avoid the common confusion with Item.Value (which does NOT exist here).
     */

    // Read int from CloudSave dictionary by key. If missing or wrong type -> return defaultValue.
    public static int ReadInt(Dictionary<string, CloudSaveItem> data, string key, int defaultValue)
    {
        if (data == null) return defaultValue;

        if (data.TryGetValue(key, out var item))
        {
            try
            {
                return item.Value.GetAs<int>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ReadInt failed for key '{key}': {e.Message}");
                return defaultValue;
            }
        }

        return defaultValue;
    }

    // Read string from CloudSave dictionary by key. If missing or wrong type -> return defaultValue.
    public static string ReadString(Dictionary<string, CloudSaveItem> data, string key, string defaultValue)
    {
        if (data == null) return defaultValue;

        if (data.TryGetValue(key, out var item))
        {
            try
            {
                return item.Value.GetAs<string>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ReadString failed for key '{key}': {e.Message}");
                return defaultValue;
            }
        }

        return defaultValue;
    }
}
