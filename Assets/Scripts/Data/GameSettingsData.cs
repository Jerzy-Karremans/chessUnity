public enum EnemyType
{
    SingleScreen,
    Bot,
    Multiplayer
}

public static class GameSettingsData
{
    public static EnemyType enemyType = EnemyType.Bot;
    public static bool playingAsWhite = true;
    public static bool gameOver = false;
}
