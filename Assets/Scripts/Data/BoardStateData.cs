
using System;

public enum GameStage
{
    Default,
    Promotion,
    GameOver,
    WaitingForOpponent,
    OpponentDisconnected
};

[Serializable]
public class BoardStateData
{
    public int[,] board = new int[8, 8];
    public bool[,] castleMoved = new bool[2, 2];
    public int enPassantColumn = -1; //-1 if there is no enPessant possible next move
    public bool isWhiteTurn = true;
    public MoveData lastMove;
    public int moveCount = 0;
}
