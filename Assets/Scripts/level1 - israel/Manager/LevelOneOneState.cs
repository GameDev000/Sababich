/// <summary>
/// Holds the state of Level 1.1 regarding success status
/// and served dishes statistics.
/// </summary>
public static class LevelOneOneState
{
    // Whether the level objective was completed successfully
    public static bool IsSuccess;

    // Total number of dishes served during the level
    public static int TotalServedDishes;

    // Number of dishes that were served with 100% perfect match
    public static int PerfectServedDishes;

    // Number of duplicate ingredient clicks during the level
    public static int DuplicateIngredientClicks;

    // Number of times the gluten-sensitive child appeared during the level
    public static int GlutenChildAppeared;

    // Number of times the gluten-sensitive child was served during the level
    public static int GlutenChildServed;

    // Number of customers that arrived during the level
    public static int CustomersArrived;

    /// <summary>
    /// Resets all level statistics and success status.
    /// </summary>
    public static void Reset()
    {
        IsSuccess = false;
        TotalServedDishes = 0;
        PerfectServedDishes = 0;
        DuplicateIngredientClicks = 0;
        GlutenChildAppeared = 0;
        GlutenChildServed = 0;
        CustomersArrived = 0;
    }
}