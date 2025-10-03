using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoardRenderer))]
[RequireComponent(typeof(ChessGameStateModel))]
[RequireComponent(typeof(PieceRenderer))]
[RequireComponent(typeof(AudioRenderer))]
public class GameController : MonoBehaviour
{
    private ChessGameStateModel ChessGameStateModel;
    private BoardRenderer BoardRenderer;
    private PieceRenderer PieceRenderer;
    private AudioRenderer AudioRenderer;

    void Start()
    {
        ChessGameStateModel = GetComponent<ChessGameStateModel>();
        BoardRenderer = GetComponent<BoardRenderer>();
        BoardRenderer.InitializeSquares();
        PieceRenderer = GetComponent<PieceRenderer>();
        PieceRenderer.DrawPieces(ChessGameStateModel.GetInitialBoard().board);
        AudioRenderer = GetComponent<AudioRenderer>();
    }

    public bool OnSquareClicked(Vector2Int pos)
    {
        GameObject piece = PieceRenderer.GetPieceAt(pos);
        if (piece == null)
            return false;
        if (ChessGameStateModel.GetIswhiteTurn() != ChessGameStateModel.IsWhitePiece(pos))
            return false;            

        // hover current piece
        piece.transform.localScale = Vector3.one * 0.9f;
        PieceRenderer.HoveringPiece = piece;

        // draw move options
        BoardRenderer.DrawPossibleMoves(ChessGameStateModel.GetPossibleMoves(pos));

        return true;
    }

    public void OnPieceMoved(Vector2Int pos, Vector2Int newPos)
    {
        BoardRenderer.ClearPossibleMovesIndicator();

        int pieceIndex = ChessGameStateModel.GetPieceIndex(pos);
        BoardStateData updatedBoard = ChessGameStateModel.OnMove(pos, newPos, pieceIndex);
        PieceRenderer.DrawPieces(updatedBoard.board);

        MoveData lastMove = updatedBoard.lastMove;
        if (lastMove == null) return;

        BoardRenderer.DrawLastMoveIndicators(lastMove);
        AudioRenderer.PlayMoveSound(lastMove);
    }
}
