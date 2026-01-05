/// <summary>
/// Holds the state of Level Two regarding success status and served dishes statistics.
/// </summary>
public static class LevelTwoState
{
    // Whether the level objective was completed successfully
    public static bool IsSuccess;

    // Total number of dishes served during the level
    public static int TotalServedDishes;

    // Number of dishes that were served with 100% perfect match
    public static int PerfectServedDishes;

    public static void Reset()
    {
        IsSuccess = false;
        TotalServedDishes = 0;
        PerfectServedDishes = 0;
    }
}
