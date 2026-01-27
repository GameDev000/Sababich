using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

/**
 * Handles Unity Gaming Services initialization and username+password authentication.
 * Note: PlayerId is internal/debug info - do NOT show it in UI messages.
 */
public class AuthenticationManagerWithPassword : MonoBehaviour
{
    // Initializing the Unity Services SDK
    private async void Awake()
    {
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
}
