using UnityEngine;

public static class GameSettings
{
    public enum EnemyType
    {
        SingleScreen,
        RandomBot,
        Multiplayer,
        OpponentDisconnected
    }

    static public EnemyType enemyType = EnemyType.Multiplayer;
    public static bool boardFlipped = false;
    public static string player1Name = "player1";
    public static string player2Name = "player2";
}
