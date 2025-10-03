using UnityEngine;
using UnityEngine.Analytics;

public static class ChessRules
{
    public static bool CanPieceMoveToPosition(BoardStateData boardState, MoveData moveData)
    {
        Vector2Int currentPos = moveData.pos;
        Vector2Int newPos = moveData.newPos;
        Vector2Int pos = moveData.pos;
        int[,] board = boardState.board;
        bool isWhiteTurn = boardState.isWhiteTurn;

        // within the game board
        if (newPos.x < 0 || newPos.x > 7 || newPos.y < 0 || newPos.y > 7) return false;
        int currentPiece = board[pos.y, pos.x];
        int targetPiece = board[newPos.y, newPos.x];
        // not itself
        if (newPos.x == currentPos.x && newPos.y == currentPos.y) return false;
        if (targetPiece != 0)
        {
            // Can't capture your own piece
            if (IsWhitePiece(newPos, board) == isWhiteTurn) return false;
            // Can't capture kings
            if (targetPiece == 5 || targetPiece == 11) return false;
        }

        bool isValidMove = currentPiece switch
        {
            1 or 7 => RookIsValid(currentPos, newPos, board),
            2 or 8 => KnightIsvalid(currentPos, newPos),
            3 or 9 => BishopIsValid(currentPos, newPos, board),
            4 or 10 => BishopIsValid(currentPos, newPos, board) || RookIsValid(currentPos, newPos, board), // queen
            5 or 11 => KingIsValid(currentPos, newPos, board, boardState.castleMoved, isWhiteTurn),
            6 or 12 => PawnIsValid(currentPos, newPos, isWhiteTurn, board, boardState.enPassantColumn),
            _ => false // No piece or invalid piece type
        };

        if (!isValidMove) return false;

        int[,] tempBoard = (int[,])board.Clone();
        tempBoard[newPos.y, newPos.x] = currentPiece;
        tempBoard[pos.y, pos.x] = 0;

        HandleEnPassant(moveData, currentPiece, tempBoard, boardState.enPassantColumn);

        HandleCastling(moveData, currentPiece, tempBoard);

        return !IsChecked(isWhiteTurn, tempBoard, boardState.castleMoved, boardState.enPassantColumn);
    }

    // Validation helpers
    private static bool KnightIsvalid(Vector2Int pos, Vector2Int newPos)
    {
        if (Mathf.Abs(pos.x - newPos.x) == 2 && Mathf.Abs(pos.y - newPos.y) == 1 || Mathf.Abs(pos.x - newPos.x) == 1 && Mathf.Abs(pos.y - newPos.y) == 2) return true;
        return false;
    }

    private static bool RookIsValid(Vector2Int pos, Vector2Int newPos, int[,] boardState)
    {
        if (pos.x != newPos.x && pos.y != newPos.y) return false;
        return IsPathClear(pos, newPos, boardState);
    }

    private static bool BishopIsValid(Vector2Int pos, Vector2Int newPos, int[,] boardState)
    {
        if (Mathf.Abs(newPos.x - pos.x) != Mathf.Abs(newPos.y - pos.y)) return false;
        return IsPathClear(pos, newPos, boardState);
    }

    private static bool KingIsValid(Vector2Int pos, Vector2Int newPos, int[,] boardState, bool[,] castleMoved, bool isWhiteTurn)
    {
        int colorIndex = isWhiteTurn ? 0 : 1;
        
        if (newPos.y == colorIndex * 7) 
        {
            // King side castle
            if (newPos.x == 6 &&
                (boardState[colorIndex * 7, 7] == 1 || boardState[colorIndex * 7, 7] == 7) &&
                boardState[newPos.y, 5] == 0 &&
                !castleMoved[colorIndex, 1]) // This check is sufficient
            {
                return true;
            }

            // Queen side castle
            if (newPos.x == 2 &&
                (boardState[colorIndex * 7, 0] == 1 || boardState[colorIndex * 7, 0] == 7) && // Rook present
                boardState[newPos.y, 1] == 0 && // Squares between king and rook empty
                boardState[newPos.y, 3] == 0 &&
                !castleMoved[colorIndex, 0]) // Queen-side castling available
            {
                return true;
            }
        }

        return Mathf.Abs(newPos.x - pos.x) <= 1 && Mathf.Abs(newPos.y - pos.y) <= 1;
    }

    private static bool PawnIsValid(Vector2Int pos, Vector2Int newPos, bool isWhitePawn, int[,] board, int enPessantColumn)
    {
        int direction = isWhitePawn ? 1 : -1;
        int startingRow = isWhitePawn ? 1 : 6;

        // normal movement
        if (pos.x == newPos.x && board[newPos.y, newPos.x] == 0)
        {
            // one square forward
            if (pos.y + direction == newPos.y) return true;

            // too squares forward with in between empty
            if (pos.y == startingRow && newPos.y == startingRow + 2 * direction &&
                    board[pos.y + direction, pos.x] == 0) return true;
        }

        // Attacks
        if (Mathf.Abs(newPos.x - pos.x) == 1 && pos.y + direction == newPos.y)
        {
            // regulair capture
            if (board[newPos.y, newPos.x] != 0) return true;

            if (enPessantColumn != newPos.x) return false;

            int pawnRow = isWhitePawn ? 4 : 3; // Row where pawns can capture en passant

            if (pos.y != pawnRow) return false;

            int enemyPawn = board[pawnRow, newPos.x];
            if (enemyPawn == 0) return false;

            return enemyPawn == 6 != isWhitePawn;
        }

        return false;
    }

    public static bool IsChecked(bool whiteTurn, int[,] board, bool[,] castleMoved = null, int enPassantColumn = -1)
    {
        int targetKing = whiteTurn ? 5 : 11;
        bool isChecked = false;

        ConverterUtils.ForEachSquare((row, col) =>
        {
            if (isChecked) return;

            if (board[row, col] == targetKing)
                isChecked = SquareUnderAttack(new Vector2Int(col, row), whiteTurn, board, castleMoved, enPassantColumn);
        });

        return isChecked;
    }

    public static bool SquareUnderAttack(Vector2Int pos, bool whiteTurn, int[,] board, bool[,] castleMoved = null, int enPassantColumn = -1)
    {
        bool isUnderAttack = false;

        ConverterUtils.ForEachSquare((row, col) =>
        {
            if (isUnderAttack) return;

            int enemyPiece = board[row, col];
            if (enemyPiece == 0 || IsWhitePiece(new Vector2Int(col, row), board) == whiteTurn)
                return;

            Vector2Int enemyPos = new(col, row);
            int direction = IsWhitePiece(pos, board) ? 1 : -1;
            bool canAttack = enemyPiece switch
            {
                1 or 7 => RookIsValid(enemyPos, pos, board),
                2 or 8 => KnightIsvalid(enemyPos, pos),
                3 or 9 => BishopIsValid(enemyPos, pos, board),
                4 or 10 => BishopIsValid(enemyPos, pos, board) || RookIsValid(enemyPos, pos, board),
                5 or 11 => Mathf.Abs(col - pos.x) <= 1 && Mathf.Abs(row - pos.y) <= 1,
                6 or 12 => Mathf.Abs(pos.x - enemyPos.x) == 1 && pos.y + direction == enemyPos.y,
                _ => false
            };

            if (canAttack) isUnderAttack = true;
        });

        return isUnderAttack;
    }

    // Helper methods
    private static bool IsPathClear(Vector2Int from, Vector2Int to, int[,] boardState)
    {
        // Calculate direction vector
        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        // Get the number of steps (should be same for both axes in diagonal moves)
        int steps = Mathf.Max(Mathf.Abs(deltaX), Mathf.Abs(deltaY));

        // Calculate step direction (-1, 0, or 1 for each axis)
        int stepX = deltaX == 0 ? 0 : deltaX / Mathf.Abs(deltaX);
        int stepY = deltaY == 0 ? 0 : deltaY / Mathf.Abs(deltaY);

        // Check each square along the path (excluding start and end)
        for (int i = 1; i < steps; i++)
        {
            int checkX = from.x + (stepX * i);
            int checkY = from.y + (stepY * i);

            if (boardState[checkY, checkX] != 0)
                return false; // Path is blocked
        }
        return true;
    }

    public static bool IsWhitePiece(Vector2Int pos, int[,] board)
    {
        int pieceType = board[pos.y, pos.x];
        return pieceType >= 1 && pieceType <= 6;
    }

    public static void HandleCastling(MoveData newMove, int pieceIndex, int[,] board)
    {
        var pos = newMove.pos;
        var newPos = newMove.newPos;

        // King moves 2 squares during casteling
        if (IsKing(pieceIndex) && Mathf.Abs(newPos.x - pos.x) == 2)
        {
            bool isKingSide = newPos.x > pos.x;
            int rookFromX = isKingSide ? 7 : 0;
            int rookToX = isKingSide ? newPos.x - 1 : newPos.x + 1;

            // Move the rook
            board[pos.y, rookToX] = board[pos.y, rookFromX];
            board[pos.y, rookFromX] = 0;
            newMove.MoveSpeciality = MoveSpeciality.isCastling;
        }
    }

    public static void HandleEnPassant(MoveData newMove, int pieceIndex, int[,] board, int enPassantColumn)
    {
        var pos = newMove.pos;
        var newPos = newMove.newPos;

        if (IsPawn(pieceIndex) &&
            Mathf.Abs(newPos.x - pos.x) == 1 &&
            board[newPos.y, newPos.x] == 0 &&
            enPassantColumn == newPos.x)
        {
            // remove the captured pawn
            board[pos.y, newPos.x] = 0;
            newMove.MoveSpeciality = MoveSpeciality.isCapture;
        }
    }
    
    public static bool IsPawn(int pieceType) => pieceType == 6 || pieceType == 12;
    public static bool IsKing(int pieceType) => pieceType == 5 || pieceType == 11;
    public static bool IsRook(int pieceType) => pieceType == 1 || pieceType == 7;
}