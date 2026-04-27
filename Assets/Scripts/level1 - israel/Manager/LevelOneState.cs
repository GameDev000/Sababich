/// <summary>
/// Holds the state of Level One and Level Three regarding success status
/// and served dishes statistics.
/// </summary>
public static class LevelOneState
{
    // Whether the level objective was completed successfully
    public static bool IsSuccess;

    // Total number of dishes served during the level
    public static int TotalServedDishes;

    // Number of dishes that were served with 100% perfect match
    public static int PerfectServedDishes;

    public static int DuplicateIngredientClicks;
    public static int GlutenChildAppeared;
    public static int GlutenChildServed;

    public static void Reset()
    {
        IsSuccess = false;
        TotalServedDishes = 0;
        PerfectServedDishes = 0;
        DuplicateIngredientClicks = 0;
        GlutenChildAppeared = 0;
        GlutenChildServed = 0;
    }
}

public static class LevelThreeState
{
    // Whether the level objective was completed successfully
    public static bool IsSuccess;

    // Total number of dishes served during the level
    public static int TotalServedDishes;

    // Number of dishes that were served with 100% perfect match
    public static int PerfectServedDishes;

    public static int DuplicateIngredientClicks;
    public static int GlutenChildAppeared;
    public static int GlutenChildServed;

    public static void Reset()
    {
        IsSuccess = false;
        TotalServedDishes = 0;
        PerfectServedDishes = 0;
        DuplicateIngredientClicks = 0;
        GlutenChildAppeared = 0;
        GlutenChildServed = 0;
    }
}
