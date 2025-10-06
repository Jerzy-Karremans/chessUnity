using UnityEngine;

public enum MoveSpeciality
{
    Default,
    isCapture,
    isCastling,
    isCheck,
    isPromotion
}

public enum GameState
{
    Default,
    WhiteCheckmate,
    BlackCheckmate,
    Stalemate,
    Draw,
    WhiteResigned,
    BlackResigned
}

public class MoveData
{
    public Vector2Int pos;
    public Vector2Int newPos;
    public int pieceIndex;
    public int promotionPieceIndex = -1;
    public MoveSpeciality MoveSpeciality;
    public GameState gameState;

    public MoveData(Vector2Int pos, Vector2Int newPos, int pieceIndex = 0, MoveSpeciality MoveSpeciality = MoveSpeciality.Default, GameState gameState = GameState.Default)
    {
        this.pos = pos;
        this.newPos = newPos;
        this.pieceIndex = pieceIndex;
        this.MoveSpeciality = MoveSpeciality;
        this.gameState = gameState;
    }
}
