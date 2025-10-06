using UnityEngine;

public class ChessGameStateModel : MonoBehaviour
{
    private BoardStateData BoardStateData;

    public void OnEnable()
    {
        BoardStateData = GetInitialBoard();
    }

    public BoardStateData OnMove(Vector2Int pos, Vector2Int newPos, int pieceIndex)
    {
        MoveData newMove = new(pos, newPos, pieceIndex);
        BoardStateData.lastMove = null;
        if (!ChessRules.CanPieceMoveToPosition(BoardStateData, newMove))
            return BoardStateData;

        BoardStateData.lastMove = MovePiece(newMove, pieceIndex);
        BoardStateData.isWhiteTurn = !BoardStateData.isWhiteTurn;

        return BoardStateData;
    }

    private MoveData MovePiece(MoveData newMove, int pieceIndex)
    {
        var newPos = newMove.newPos;
        var pos = newMove.pos;
        var board = BoardStateData.board;

        ChessRules.HandleEnPassant(newMove, pieceIndex, board, BoardStateData.enPassantColumn);
        ChessRules.HandleCastling(newMove, pieceIndex, board);

        if (board[newPos.y, newPos.x] != 0)
            newMove.MoveSpeciality = MoveSpeciality.isCapture;

        board[newPos.y, newPos.x] = board[pos.y, pos.x];
        board[pos.y, pos.x] = 0;

        //check double pawn move for enpassant
        if (ChessRules.IsPawn(pieceIndex) && Mathf.Abs(newPos.y - pos.y) == 2)
            BoardStateData.enPassantColumn = pos.x;
        else
            BoardStateData.enPassantColumn = -1;

        if (ChessRules.IsKing(pieceIndex) || ChessRules.IsRook(pieceIndex))
            UpdateRookCastlingRights(pos, pieceIndex);

        if (ChessRules.IsPawn(pieceIndex) && (newPos.y == 0 || newPos.y == 7))
        {
            newMove.MoveSpeciality = MoveSpeciality.isPromotion;
            return newMove;
        }

        UpdateGameState(newMove);
        return newMove;
    }

    public BoardStateData PromotePawn(Vector2Int pos, int promotionPieceIndex)
    {
        BoardStateData.board[pos.y, pos.x] = promotionPieceIndex;
        BoardStateData.lastMove.promotionPieceIndex = promotionPieceIndex;
        BoardStateData.lastMove.pieceIndex = promotionPieceIndex;
        UpdateGameState(BoardStateData.lastMove, !BoardStateData.isWhiteTurn);
        return BoardStateData;
    }

    private void UpdateGameState(MoveData newMove, bool? opponentTurn = null)
    {
        bool checkingPlayer = opponentTurn ?? !BoardStateData.isWhiteTurn;

        bool opponentInCheck = ChessRules.IsChecked(!checkingPlayer,
            BoardStateData.board, BoardStateData.castleMoved, BoardStateData.enPassantColumn);

        if (opponentInCheck)
        {
            newMove.MoveSpeciality = MoveSpeciality.isCheck;
            
            if (ChessRules.IsCheckMate(!checkingPlayer, BoardStateData.board, BoardStateData.castleMoved, BoardStateData.enPassantColumn))
            {
                newMove.gameState = checkingPlayer ? GameState.WhiteCheckmate : GameState.BlackCheckmate;
            }
        }
        else
        {
            if (ChessRules.IsStalemate(!checkingPlayer, BoardStateData.board, BoardStateData.castleMoved, BoardStateData.enPassantColumn))
                newMove.gameState = GameState.Stalemate;
        }

    }

    private void UpdateRookCastlingRights(Vector2Int pos, int pieceIndex)
    {
        int colorIndex = pieceIndex <= 6 ? 0 : 1;

        if (ChessRules.IsKing(pieceIndex))
        {
            BoardStateData.castleMoved[colorIndex, 0] = true;
            BoardStateData.castleMoved[colorIndex, 1] = true;
        }
        else
        {
            int expectedRow = colorIndex == 0 ? 0 : 7;

            if (pos.y == expectedRow)
            {
                if (pos.x == 0)
                    BoardStateData.castleMoved[colorIndex, 0] = true;
                else if (pos.x == 7)
                    BoardStateData.castleMoved[colorIndex, 1] = true;
            }
        }
    }

    public bool GetIswhiteTurn()
    {
        return BoardStateData.isWhiteTurn;
    }

    public int[,] GetBoardState()
    {
        return BoardStateData.board;
    }

    public static BoardStateData GetInitialBoard()
    {
        BoardStateData newBoard = new();
        int[] backRank = { 1, 2, 3, 4, 5, 3, 2, 1 };

        ConverterUtils.ForEachSquare((row, col) =>
        {
            int piece = row switch
            {
                0 => backRank[col],
                1 => 6,
                6 => 12,
                7 => backRank[col] + 6,
                _ => 0
            };
            newBoard.board[row, col] = piece;
        });

        newBoard.castleMoved = new[,] { { false, false }, { false, false } };

        newBoard.lastMove = null;

        return newBoard;
    }

    public bool IsWhitePiece(Vector2Int pos)
    {
        return ChessRules.IsWhitePiece(pos, BoardStateData.board);
    }

    public int[,] GetPossibleMoves(Vector2Int piecePos)
    {
        int[,] couldMoveTo = new int[8, 8];

        ConverterUtils.ForEachSquare((row, col) =>
        {
            MoveData possiblemove = new(piecePos, new Vector2Int(col, row));

            if (ChessRules.CanPieceMoveToPosition(BoardStateData, possiblemove))
                couldMoveTo[row, col] = 1;
            else
                couldMoveTo[row, col] = 0;
        });

        return couldMoveTo;
    }

    public int GetPieceIndex(Vector2Int pos)
    {
        return BoardStateData.board[pos.y, pos.x];
    }
}