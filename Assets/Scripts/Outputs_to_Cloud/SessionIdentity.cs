public static class SessionIdentity
{
    public static string DisplayName { get; private set; }
    public static string InternalUsername { get; private set; }
    public static bool IsGuest { get; private set; }

    public static void SetFromLogin(string displayName, string internalUsername, bool isGuest)
    {
        DisplayName = displayName;
        InternalUsername = internalUsername;
        IsGuest = isGuest;
    }

    public static void Clear()
    {
        DisplayName = null;
        InternalUsername = null;
        IsGuest = false;
    }

    public static string GenerateSessionId()
    {
        string user = string.IsNullOrEmpty(InternalUsername) ? "unknown" : InternalUsername;
        return $"{user}_{System.DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}
