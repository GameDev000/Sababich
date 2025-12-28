using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class CloudStateSync : MonoBehaviour
{
    // Call this after successful signin
    public async Task SyncStatesFromCloud()
    {
        // No cloud -> keep local behavior (new user / offline)
        if (UnityServices.State != ServicesInitializationState.Initialized ||
            !AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[CloudStateSync] Cloud not ready -> keep local states.");
            return;
        }

        var data = await DatabaseManager.LoadData("level1_passed", "level2_passed", "level3_passed");

        // New user no data -> keep local states
        if (data == null || data.Count == 0)
        {
            Debug.Log("[CloudStateSync] No cloud data -> new user -> keep local states.");
            return;
        }

        int l1 = DatabaseManager.ReadInt(data, "level1_passed", LevelOneState.IsSuccess ? 1 : 0);
        int l2 = DatabaseManager.ReadInt(data, "level2_passed", LevelTwoState.IsSuccess ? 1 : 0);
        int l3 = DatabaseManager.ReadInt(data, "level3_passed", LevelThreeState.IsSuccess ? 1 : 0);

        LevelOneState.IsSuccess = (l1 == 1);
        LevelTwoState.IsSuccess = (l2 == 1);
        LevelThreeState.IsSuccess = (l3 == 1);

        Debug.Log($"[CloudStateSync] Synced: L1={l1}, L2={l2}, L3={l3}");
    }
}
