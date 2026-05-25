/// <summary>
/// Holds the state of Level 3.1 USA regarding success status and served dishes statistics.
/// </summary>
public static class LevelTwoTwoState
{
    // Whether the level objective was completed successfully
    public static bool IsSuccess;

    // Total number of dishes served during the level
    public static int TotalServedDishes;

    // Number of dishes that were served with 100% perfect match
    public static int PerfectServedDishes;

    // Number of duplicate ingredient clicks during the level
    public static int DuplicateIngredientClicks;

    // Number of gluten-sensitive children that appeared during the level
    public static int GlutenChildAppeared;

    // Number of gluten-sensitive children that were served during the level
    public static int GlutenChildServed;

    // Total number of customers that arrived during the level
    public static int CustomersArrived;

    /// <summary>
    /// Resets all level state values before starting or replaying Level 3.1 USA.
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