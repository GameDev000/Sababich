// using System.Threading.Tasks;
// using Unity.Services.Authentication;
// using Unity.Services.Core;
// using UnityEngine;

// /**
//  * Handles Unity Gaming Services initialization and username+password authentication.
//  * Note: PlayerId is internal/debug info - do NOT show it in UI messages.
//  */
// public class AuthenticationManagerWithPassword : MonoBehaviour
// {
//     // Initializing the Unity Services SDK
//     private async void Awake()
//     {
//         Debug.Log("AuthenticationManagerWithPassword Awake");
//         await UnityServices.InitializeAsync();

//         // Debug only (Console). This is NOT shown in the UI unless you explicitly put it in a return string.
//         if (AuthenticationService.Instance.IsSignedIn)
//         {
//             Debug.Log($"Player is already signed in. PlayerId: {AuthenticationService.Instance.PlayerId}");
//         }
//         else
//         {
//             Debug.Log("Player is not signed in yet - waiting for sign-in");
//         }
//     }

//     /**
//      * Sign up a new user with username and password.
//      * Returns a user-friendly success/error message (without PlayerId).
//      */
//     public async Task<string> RegisterWithUsernameAndPassword(string username, string password)
//     {
//         try
//         {
//             await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

//             // Debug only
//             Debug.Log($"Register successful. PlayerId: {AuthenticationService.Instance.PlayerId}");

//             // UI message (NO PlayerId)
//             return "Register successful!";
//         }
//         catch (AuthenticationException ex)
//         {
//             return $"Register failed: {ex.Message}";
//         }
//         catch (RequestFailedException ex)
//         {
//             return $"Register request failed: {ex.Message}";
//         }
//     }

//     /**
//      * Sign in an existing user with username and password.
//      * Returns a user-friendly success/error message (without PlayerId).
//      */
//     public async Task<string> LoginWithUsernameAndPassword(string username, string password)
//     {
//         try
//         {
//             await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

//             // Debug only
//             Debug.Log($"Login successful. PlayerId: {AuthenticationService.Instance.PlayerId}");

//             // UI message (NO PlayerId)
//             return "Login successful!";
//         }
//         catch (AuthenticationException ex)
//         {
//             return $"Login failed: {ex.Message}";
//         }
//         catch (RequestFailedException ex)
//         {
//             return $"Login request failed: {ex.Message}";
//         }
//     }

//     /**
//      * Sign in as a guest (anonymous) user.
//      * Returns a user-friendly success/error message (without PlayerId).
//      */
//     public async Task<string> LoginAnonymously()
//     {
//         try
//         {
//             // If already signed in, avoid throwing
//             if (AuthenticationService.Instance.IsSignedIn)
//             {
//                 // Debug only
//                 Debug.Log($"Guest already signed in. PlayerId: {AuthenticationService.Instance.PlayerId}");

//                 // UI message (NO PlayerId)
//                 return "Guest already signed in.";
//             }

//             await AuthenticationService.Instance.SignInAnonymouslyAsync();

//             // Debug only
//             Debug.Log($"Guest login successful. PlayerId: {AuthenticationService.Instance.PlayerId}");

//             // UI message (NO PlayerId)
//             return "Guest login successful!";
//         }
//         catch (AuthenticationException ex)
//         {
//             return $"Guest login failed: {ex.Message}";
//         }
//         catch (RequestFailedException ex)
//         {
//             return $"Guest login request failed: {ex.Message}";
//         }
//     }

//     /**
//      * Sign out the current user.
//      */
//     public void SignOut()
//     {
//         AuthenticationService.Instance.SignOut();
//         Debug.Log("Player signed out");
//     }
// }


using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

// ===== NEW (Delete user cloud + account) =====
using Unity.Services.CloudSave; // NEW
// ============================================

/**
 * Handles Unity Gaming Services initialization and username+password authentication.
 * Note: PlayerId is internal/debug info - do NOT show it in UI messages.
 */
public class AuthenticationManagerWithPassword : MonoBehaviour
{
    // Initializing the Unity Services SDK
    private async void Awake()
    {
        // ================================
        // NEW: keep manager across scenes
        // ================================
        DontDestroyOnLoad(gameObject); // NEW
        // ================================

        Debug.Log("AuthenticationManagerWithPassword Awake");
        await UnityServices.InitializeAsync();

        // Debug only (Console). This is NOT shown in the UI unless you explicitly put it in a return string.
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log($"Player is already signed in. PlayerId: {AuthenticationService.Instance.PlayerId}");
        }
        else
        {
            Debug.Log("Player is not signed in yet - waiting for sign-in");
        }
    }

    /**
     * Sign up a new user with username and password.
     * Returns a user-friendly success/error message (without PlayerId).
     */
    public async Task<string> RegisterWithUsernameAndPassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

            // Debug only
            Debug.Log($"Register successful. PlayerId: {AuthenticationService.Instance.PlayerId}");

            // UI message (NO PlayerId)
            return "Register successful!";
        }
        catch (AuthenticationException ex)
        {
            return $"Register failed: {ex.Message}";
        }
        catch (RequestFailedException ex)
        {
            return $"Register request failed: {ex.Message}";
        }
    }

    /**
     * Sign in an existing user with username and password.
     * Returns a user-friendly success/error message (without PlayerId).
     */
    public async Task<string> LoginWithUsernameAndPassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            // Debug only
            Debug.Log($"Login successful. PlayerId: {AuthenticationService.Instance.PlayerId}");

            // UI message (NO PlayerId)
            return "Login successful!";
        }
        catch (AuthenticationException ex)
        {
            return $"Login failed: {ex.Message}";
        }
        catch (RequestFailedException ex)
        {
            return $"Login request failed: {ex.Message}";
        }
    }

    /**
     * Sign in as a guest (anonymous) user.
     * Returns a user-friendly success/error message (without PlayerId).
     */
    public async Task<string> LoginAnonymously()
    {
        try
        {
            // If already signed in, avoid throwing
            if (AuthenticationService.Instance.IsSignedIn)
            {
                // Debug only
                Debug.Log($"Guest already signed in. PlayerId: {AuthenticationService.Instance.PlayerId}");

                // UI message (NO PlayerId)
                return "Guest already signed in.";
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // Debug only
            Debug.Log($"Guest login successful. PlayerId: {AuthenticationService.Instance.PlayerId}");

            // UI message (NO PlayerId)
            return "Guest login successful!";
        }
        catch (AuthenticationException ex)
        {
            return $"Guest login failed: {ex.Message}";
        }
        catch (RequestFailedException ex)
        {
            return $"Guest login request failed: {ex.Message}";
        }
    }

    /**
     * Sign out the current user.
     */
    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();
        Debug.Log("Player signed out");
    }

    // =========================================================
    // NEW: Delete user from cloud (Cloud Save) + delete account
    // =========================================================
    /**
     * Deletes the current player's Cloud Save data AND the Authentication account,
     * then signs out (so your UI can return to Login screen).
     *
     * IMPORTANT:
     * - Cloud Save data is deleted first.
     * - Then the Authentication account is deleted.
     * - Finally we sign out + clear session token to force login screen.
     */
    public async Task<string> DeleteCurrentUserFromCloudAndSignOut() // NEW
    {
        try
        {
            // Safety: ensure services initialized (your Awake already does this, but keep it safe)
            if (UnityServices.State != ServicesInitializationState.Initialized) // NEW
            {
                await UnityServices.InitializeAsync(); // NEW
            }

            // Must be signed in to delete player data / account
            if (!AuthenticationService.Instance.IsSignedIn) // NEW
            {
                // Still clear local token so UI goes to login
                AuthenticationService.Instance.ClearSessionToken(); // NEW
                return "No signed-in user to delete.";
            }

            // 1) Delete ALL Cloud Save keys for this player
            await CloudSaveService.Instance.Data.Player.DeleteAllAsync(); // NEW
            Debug.Log("[DeleteUser] Cloud Save data deleted."); // NEW

            // 2) Delete Authentication account (permanent)
            await AuthenticationService.Instance.DeleteAccountAsync(); // NEW
            Debug.Log("[DeleteUser] Authentication account deleted."); // NEW

            // 3) Sign out + clear credentials locally so app returns to login flow
            AuthenticationService.Instance.SignOut(true); // NEW (clearCredentials=true)
            AuthenticationService.Instance.ClearSessionToken(); // NEW

            return "User deleted successfully.";
        }
        catch (AuthenticationException ex) // NEW
        {
            return $"Delete user failed: {ex.Message}";
        }
        catch (RequestFailedException ex) // NEW
        {
            return $"Delete user request failed: {ex.Message}";
        }
    }
}