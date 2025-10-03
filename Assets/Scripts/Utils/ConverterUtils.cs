using UnityEngine;

public class ConverterUtils
{
    public static float BOARD_OFFSET = 3.5f;
    public static Vector3 GetRealPos(Vector2Int gamePos, float layerHeight)
    {
        float newX = gamePos.x - BOARD_OFFSET;
        float newY = (gamePos.y - BOARD_OFFSET) * (GameSettingsData.playingAsWhite ? 1 : -1);
        return new Vector3(newX, newY, -layerHeight);
    }

    public static Vector2Int GetGamePos(Vector3 realPos)
    {
        float gameX = realPos.x + BOARD_OFFSET + 0.5f;
        float gameY = (realPos.y * (GameSettingsData.playingAsWhite ? 1 : -1)) + BOARD_OFFSET + 0.5f;
        return new Vector2Int(Mathf.FloorToInt(gameX), Mathf.FloorToInt(gameY));
    }

    public static void ForEachSquare(System.Action<int, int> action)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                action(row, col);
            }
        }
    }
}
