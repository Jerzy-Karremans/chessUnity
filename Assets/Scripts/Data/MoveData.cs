using UnityEngine;

public enum MoveSpeciality
{
    Default,
    isCapture,
    isCastling,
    isCheck,

}

public class MoveData
{
    public Vector2Int pos;
    public Vector2Int newPos;
    public int pieceIndex;
    public MoveSpeciality MoveSpeciality;

    public MoveData(Vector2Int pos, Vector2Int newPos, int pieceIndex = 0, MoveSpeciality MoveSpeciality = MoveSpeciality.Default)
    {
        this.pos = pos;
        this.newPos = newPos;
        this.pieceIndex = pieceIndex;
        this.MoveSpeciality = MoveSpeciality;
    }
}
