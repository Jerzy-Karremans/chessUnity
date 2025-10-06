using UnityEngine;

[RequireComponent(typeof(BoardRenderer))]
[RequireComponent(typeof(ChessGameStateModel))]
[RequireComponent(typeof(PieceRenderer))]
[RequireComponent(typeof(AudioRenderer))]
[RequireComponent(typeof(UIRenderer))]
public class GameController : MonoBehaviour
{
    private ChessGameStateModel ChessGameStateModel;
    private BoardRenderer BoardRenderer;
    private PieceRenderer PieceRenderer;
    private AudioRenderer AudioRenderer;
    private UIRenderer UIRenderer;
    
    // Promotion state
    private Vector2Int promotionPos;
    private bool awaitingPromotion = false;

    void Start()
    {
        ChessGameStateModel = GetComponent<ChessGameStateModel>();
        BoardRenderer = GetComponent<BoardRenderer>();
        BoardRenderer.InitializeSquares();
        PieceRenderer = GetComponent<PieceRenderer>();
        PieceRenderer.DrawPieces(ChessGameStateModel.GetInitialBoard().board);
        AudioRenderer = GetComponent<AudioRenderer>();
        UIRenderer = GetComponent<UIRenderer>();
        UIRenderer.OnPromotionChoice += OnPromotionChoice;
    }

    public bool OnSquareClicked(Vector2Int pos)
    {
        if (awaitingPromotion) return false;

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

        // Check if this move requires promotion choice
        if (lastMove.MoveSpeciality == MoveSpeciality.isPromotion)
        {
            promotionPos = newPos;
            awaitingPromotion = true;
            UIRenderer.RenderPromotionDialog(!updatedBoard.isWhiteTurn); // Previous player's color
            return; // Don't complete the turn yet
        }

        // Complete normal move
        CompleteTurn(lastMove, updatedBoard);
    }

    private void OnPromotionChoice(int promotionPieceType, GameObject promotionDialog)
    {
        BoardStateData updatedBoard = ChessGameStateModel.PromotePawn(promotionPos, promotionPieceType);
        
        Destroy(promotionDialog);
        awaitingPromotion = false;
        
        PieceRenderer.DrawPieces(updatedBoard.board);
        
        MoveData promotionMove = updatedBoard.lastMove;
        if (promotionMove != null)
        {
            promotionMove.promotionPieceIndex = promotionPieceType;
            CompleteTurn(promotionMove, updatedBoard);
        }
    }

    private void CompleteTurn(MoveData lastMove, BoardStateData updatedBoard)
    {
        BoardRenderer.DrawLastMoveIndicators(lastMove);
        AudioRenderer.PlayMoveSound(lastMove);
        UIRenderer.SetGameStateText(lastMove);
        UIRenderer.SetTurnText(updatedBoard.isWhiteTurn);

        if (lastMove.gameState != GameState.Default)
            AudioRenderer.PlayEndGameSound(GameSettingsData.playingAsWhite == !updatedBoard.isWhiteTurn);
    }

    void OnDestroy()
    {
        if (UIRenderer != null)
            UIRenderer.OnPromotionChoice -= OnPromotionChoice;
    }
}
