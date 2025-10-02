using System;
using System.Threading;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BoardState
{
    private readonly GameObject[,] boardState = new GameObject[8, 8];
    private readonly GameObject[] WhitePieces;
    private readonly GameObject[] BlackPieces;
    private readonly bool[,] castleMoved = new bool[2, 3] { { false, false, false }, { false, false, false } };

    public BoardState(GameObject[] WhitePieces, GameObject[] BlackPieces)
    {
        this.WhitePieces = WhitePieces;
        this.BlackPieces = BlackPieces;
        InstantiatePieces();
    }
    
    public BoardState(BoardState original)
{
    this.WhitePieces = original.WhitePieces;
    this.BlackPieces = original.BlackPieces;
    
    // Deep copy the board state
    for (int row = 0; row < 8; row++)
    {
        for (int col = 0; col < 8; col++)
        {
            this.boardState[row, col] = original.boardState[row, col];
        }
    }
    
    // Copy castle moved state
    for (int i = 0; i < 2; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            this.castleMoved[i, j] = original.castleMoved[i, j];
        }
    }
    
    this.enPassantPossible = original.enPassantPossible;
}

    public bool MovePiece(Vector2Int newPos, Vector2Int pos, GameObject hoverPrefab)
    {
        bool isCapture = boardState[newPos.y, newPos.x] != null;
        // is pawn
        if (hoverPrefab == WhitePieces[5] || hoverPrefab == BlackPieces[5])
            AnPessantLogic(newPos, pos, hoverPrefab);
        // is rook or king
        if (hoverPrefab == WhitePieces[0] || hoverPrefab == WhitePieces[4] || hoverPrefab == BlackPieces[0] || hoverPrefab == BlackPieces[4])
            UpdateCastleMoved(newPos, pos, hoverPrefab);

        
        setPos(newPos.x, newPos.y, hoverPrefab);
        return isCapture;
    }

    public bool IsStalemate(bool whiteTurn)
    {

        if (IsChecked(whiteTurn)) return false;

        // Check if the current player has any legal moves
        GameObject[] currentPlayerPieces = whiteTurn ? WhitePieces : BlackPieces;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {

                GameObject piece = boardState[row, col];
                bool found = false;
                foreach (var p in currentPlayerPieces)
                {
                    if (p == piece)
                    {
                        found = true;
                        break;
                    }
                }
                if (piece == null || !found) continue;

                // Check all possible moves for this piece
                for (int targetRow = 0; targetRow < 8; targetRow++)
                {
                    for (int targetCol = 0; targetCol < 8; targetCol++)
                    {
                        if (CanPieceMoveToPosition(piece, new Vector2Int(col, row), new Vector2Int(targetCol, targetRow), whiteTurn))
                        {
                            // Test if this move would leave the king in check
                            BoardState testState = new BoardState(this);
                            testState.setPos(col, row, null);
                            testState.setPos(targetCol, targetRow, piece);

                            if (!testState.IsChecked(whiteTurn))
                            {
                                int count = 0;
                                for (int i = 0; i < 8; i++)
                                {
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if (boardState[i, j] != null) count++;
                                    }
                                }

                                return count <= 2; // Found a legal move, not stalemate
                            }
                        }
                    }
                }
            }
        }
        return true; // No legal moves found and not in check = stalemate
    }

    void UpdateCastleMoved(Vector2Int newPos, Vector2Int pos, GameObject hoverPrefab)
    {
        int colorIndex = IsWhitePiece(hoverPrefab) ? 0 : 1;
        if (newPos.y == colorIndex * 7 && !castleMoved[colorIndex, 1])
        {
            //king side castle
            if ((boardState[colorIndex * 7, 7] == WhitePieces[0] || boardState[colorIndex * 7, 7] == BlackPieces[0]) &&
              newPos.x == 6 && boardState[newPos.y, 5] == null && !castleMoved[colorIndex, 2])
            {
                boardState[colorIndex * 7, 5] = boardState[colorIndex * 7, 7];
                boardState[colorIndex * 7, 7] = null;
            }
            //queen side castle
            else if ((boardState[colorIndex * 7, 0] == WhitePieces[0] || boardState[colorIndex * 7, 0] == BlackPieces[0]) &&
                newPos.x == 2 && boardState[newPos.y, 1] == null && boardState[newPos.y, 3] == null)
            {
                boardState[colorIndex * 7, 3] = boardState[colorIndex * 7, 0];
                boardState[colorIndex * 7, 0] = null;
            }
        }

        if (pos.y == 0)
        {
            if (pos.x == 0) castleMoved[0, 0] = true;
            else if (pos.x == 4) castleMoved[0, 1] = true;
            else if (pos.x == 7) castleMoved[0, 2] = true;
        }
        else if (pos.y == 7)
        {
            if (pos.x == 0) castleMoved[1, 0] = true;
            else if (pos.x == 4) castleMoved[1, 1] = true;
            else if (pos.x == 7) castleMoved[1, 2] = true;
        }
    }

    public void setPos(int x, int y, GameObject piecePrefab)
    {
        boardState[y, x] = piecePrefab;
    }

    public GameObject getPos(int x, int y)
    {
        return boardState[y, x];
    }

    void InstantiatePieces()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = null;
                switch (row)
                {
                    case 0:
                        piece = WhitePieces[col < 5 ? col : Mathf.Abs(col - 7)];
                        break;
                    case 1:
                        piece = WhitePieces[5];
                        break;
                    case 6:
                        piece = BlackPieces[5];
                        break;
                    case 7:
                        piece = BlackPieces[col < 5 ? col : Mathf.Abs(col - 7)];
                        break;
                }

                    setPos(col, row, piece);
                
            }
        }
    }

    bool IsWhitePiece(GameObject piece)
    {
        foreach (var whitePiece in WhitePieces)
        {
            if (whitePiece == piece)
                return true;
        }
        return false;
    }


    public bool CanPieceMoveToPosition(GameObject piece, Vector2Int currentPos, Vector2Int newPos, bool whiteTurn)
    {
        // check if new pos is on game board
        if (newPos.x < 0 || newPos.x > 7 || newPos.y < 0 || newPos.y > 7) return false;
        if (newPos.x == currentPos.x && newPos.y == currentPos.y) return false;
        if (boardState[newPos.y, newPos.x] != null)
        {
            // trying to capture own piece
            if (IsWhitePiece(boardState[newPos.y, newPos.x]) == whiteTurn) return false;
            if (boardState[newPos.y, newPos.x] == BlackPieces[4] || boardState[newPos.y, newPos.x] == WhitePieces[4]) return false;
        }

        if (piece == WhitePieces[0] || piece == BlackPieces[0]) return RookIsValid(currentPos, newPos);
        else if (piece == WhitePieces[1] || piece == BlackPieces[1]) return KnightIsvalid(currentPos, newPos);
        else if (piece == WhitePieces[2] || piece == BlackPieces[2]) return BishopIsValid(currentPos, newPos);
        else if (piece == WhitePieces[3] || piece == BlackPieces[3]) return BishopIsValid(currentPos, newPos) || RookIsValid(currentPos, newPos);
        else if (piece == WhitePieces[4] || piece == BlackPieces[4]) return KingIsValid(currentPos, newPos, whiteTurn) && !SquareUnderAttack(newPos, whiteTurn);
        else if (piece == WhitePieces[5] || piece == BlackPieces[5]) return PawnIsValid(currentPos, newPos, whiteTurn);
        else throw new Exception("all pieces should be implemented, if this triggers gg ig, shaw should kill himself");
    }
    
    

    public bool IsChecked(bool whiteTurn)
    {
        // find the king pos
        GameObject targetKing = whiteTurn ? WhitePieces[4] : BlackPieces[4];

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = boardState[row, col];
                if (piece == targetKing)
                {
                    return SquareUnderAttack(new Vector2Int(col, row), whiteTurn);
                }
            }
        }
        return true;
    }

    bool SquareUnderAttack(Vector2Int pos, bool whiteTurn)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject enemyPiece = boardState[row, col];
                if (enemyPiece == null || IsWhitePiece(enemyPiece) == whiteTurn) continue;
                
                // King attack check
                if (enemyPiece == WhitePieces[4] || enemyPiece == BlackPieces[4])
                {
                    if (Mathf.Abs(col - pos.x) <= 1 && Mathf.Abs(row - pos.y) <= 1)
                        return true;
                    continue;
                } 
                    
                // Pawn attack check
                if (enemyPiece == WhitePieces[5] || enemyPiece == BlackPieces[5])
                {
                    bool isPawnWhite = IsWhitePiece(enemyPiece);
                    int direction = isPawnWhite ? 1 : -1;
                    if (Mathf.Abs(col - pos.x) == 1 && row + direction == pos.y)
                        return true;
                    continue;
                }
                // Other pieces
                if (CanPieceMoveToPosition(enemyPiece,pos, new Vector2Int(col, row), whiteTurn))
                    return true;
            }
        }
        return false;
    }

    bool KingIsValid(Vector2Int pos, Vector2Int newPos, bool WhiteTurn)
    {
        // castle check
        int colorIndex = WhiteTurn ? 0 : 1;
        if (newPos.y == colorIndex * 7 && !castleMoved[colorIndex, 1])
        {
            //king side castle
            if ((boardState[colorIndex * 7, 7] == WhitePieces[0] || boardState[colorIndex * 7, 7] == BlackPieces[0]) &&
              newPos.x == 6 && boardState[newPos.y, 5] == null && !castleMoved[colorIndex, 2])
            {
                return true;
            }
            //queen side castle
            else if ((boardState[colorIndex * 7, 0] == WhitePieces[0] || boardState[colorIndex * 7, 0] == BlackPieces[0]) &&
                newPos.x == 2 && boardState[newPos.y, 1] == null && boardState[newPos.y, 3] == null) {
                return true;
            } 
        }
        
        // normal movement
        return Mathf.Abs(newPos.x - pos.x) <= 1 && Mathf.Abs(newPos.y - pos.y) <= 1;
    }


    bool BishopIsValid(Vector2Int pos, Vector2Int newPos)
    {
        // Bishop can only move diagonally
        if (Mathf.Abs(newPos.x - pos.x) != Mathf.Abs(newPos.y - pos.y)) return false;

        // Determine direction of movement
        int xDirection = newPos.x > pos.x ? 1 : -1;
        int yDirection = newPos.y > pos.y ? 1 : -1;

        // Check all squares along the diagonal path (excluding start and end)
        int steps = Mathf.Abs(newPos.x - pos.x);
        for (int i = 1; i < steps; i++)
        {
            int checkX = pos.x + (i * xDirection);
            int checkY = pos.y + (i * yDirection);

            if (boardState[checkY, checkX] != null) return false; // Path is blocked
        }

        return true;
    }

    bool RookIsValid(Vector2Int pos, Vector2Int newPos)
    {
        // Rook can only move horizontally or vertically
        if (pos.x != newPos.x && pos.y != newPos.y) return false;

        // Check vertical movement
        if (pos.x == newPos.x)
        {
            int startY = Mathf.Min(pos.y, newPos.y);
            int endY = Mathf.Max(pos.y, newPos.y);

            // Check all squares between start and end (exclusive)
            for (int y = startY + 1; y < endY; y++)
            {
                if (boardState[y, pos.x] != null) return false; // Path is blocked
            }
        }

        // Check horizontal movement
        if (pos.y == newPos.y)
        {
            int startX = Mathf.Min(pos.x, newPos.x);
            int endX = Mathf.Max(pos.x, newPos.x);

            // Check all squares between start and end (exclusive)
            for (int x = startX + 1; x < endX; x++)
            {
                if (boardState[pos.y, x] != null) return false; // Path is blocked
            }
        }

        return true;
    }

    bool KnightIsvalid(Vector2Int pos, Vector2Int newPos)
    {
        if (Mathf.Abs(pos.x - newPos.x) == 2 && Mathf.Abs(pos.y - newPos.y) == 1 || Mathf.Abs(pos.x - newPos.x) == 1 && Mathf.Abs(pos.y - newPos.y) == 2) return true;
        return false;
    }

    int enPassantPossible = -1;

    bool PawnIsValid(Vector2Int pos, Vector2Int newPos, bool isWhitePawn)
    {
        int direction = isWhitePawn ? 1 : -1;
        int startingRow = isWhitePawn ? 1 : 6;

        // Forward movement
        if (pos.x == newPos.x && boardState[newPos.y, newPos.x] == null)
        {
            // One square forward
            if (pos.y + direction == newPos.y) return true;

            // Two squares forward from starting position
            if (pos.y == startingRow && newPos.y == startingRow + (2 * direction))
            {
                // Check that the square in between is also empty
                if (boardState[pos.y + direction, pos.x] == null)
                {
                    return true;
                }
            }
        }

        // Diagonal attack
        if (Mathf.Abs(newPos.x - pos.x) == 1 && pos.y + direction == newPos.y)
        {
            // Regular capture
            if (boardState[newPos.y, newPos.x] != null) return true;

            // En passant capture
            if (enPassantPossible != newPos.x) return false;

            int pawnRow = isWhitePawn ? 4 : 3; // Row where pawns can capture en passant
            int enemyPawnRow = isWhitePawn ? 4 : 3; // Row where enemy pawn should be

            // Check if we're on the correct row for en passant
            if (pos.y != pawnRow) return false;

            // Check if there's an enemy pawn next to us
            GameObject enemyPawn = boardState[enemyPawnRow, newPos.x];
            if (enemyPawn == null) return false;

            // Check if it's the opposite color pawn
            return IsWhitePiece(enemyPawn) != isWhitePawn;
        }

        return false;
    }
    
    void AnPessantLogic(Vector2Int newPos, Vector2Int pos, GameObject hoverPrefab)
    {
        // Check if this move enables en passant for the opponent
        bool isEnPassantMove = false;

        if ((hoverPrefab == WhitePieces[5] || hoverPrefab == BlackPieces[5]) && Mathf.Abs(newPos.y - pos.y) == 2)
        {
            enPassantPossible = newPos.x; // Set the column where en passant is possible
            isEnPassantMove = true;
        }

        // Check if this is an en passant capture (diagonal move to empty square)
        bool IsEnPassantCapture = false;
        if ((hoverPrefab == WhitePieces[5] || hoverPrefab == BlackPieces[5]) &&
            Mathf.Abs(newPos.x - pos.x) == 1 &&
            getPos(newPos.x, newPos.y) == null &&
            enPassantPossible == newPos.x)
        {
            IsEnPassantCapture = true;
        }

        // Handle en passant capture
        if (IsEnPassantCapture)
        {
            int capturedPawnRow = pos.y;
            setPos(newPos.x, capturedPawnRow, null);
        }

        // If this wasn't a pawn double move, reset en passant
        if (!isEnPassantMove)
        {
            enPassantPossible = -1;
        }
    }
}