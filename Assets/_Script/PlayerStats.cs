public static class PlayerStats
{
    public static int Lives { get; private set; } = 3;
    public static int Currency { get; private set; } = 0;
    public static float Speed { get; private set; } = 5.0f;

    public static void SetLives(int newLives) => Lives = newLives;
    public static void SetCurrency(int newCurrency) => Currency = newCurrency;
    public static void SetSpeed(float newSpeed) => Speed = newSpeed;


}