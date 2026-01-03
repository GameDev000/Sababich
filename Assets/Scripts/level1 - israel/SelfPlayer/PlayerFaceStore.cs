using UnityEngine;

public static class PlayerFaceStore
{
    public static Sprite Happy { get; private set; }
    public static Sprite Angry { get; private set; }
    public static Sprite Furious { get; private set; }

    public static bool HasAll => Happy != null && Angry != null && Furious != null;

    public static void Set(Sprite happy, Sprite angry, Sprite furious)
    {
        Happy = happy;
        Angry = angry;
        Furious = furious;
    }

    public static void Clear()
    {
        Happy = null;
        Angry = null;
        Furious = null;
    }
}
