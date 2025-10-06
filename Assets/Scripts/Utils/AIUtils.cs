using System.Collections.Generic;
using UnityEngine;

public class AIUtils : MonoBehaviour
{
    public static MoveData GetRandomMove(BoardStateData boardState)
    {
        List<MoveData> possibleMoves = new();

        // Find all possible moves for the current player
        ConverterUtils.ForEachSquare((row, col) =>
        {
            int piece = boardState.board[row, col];
            if (piece == 0) return; // Empty square
            
            // Check if this piece belongs to the current player
            if (ChessRules.IsWhitePiece(new Vector2Int(col, row), boardState.board) != boardState.isWhiteTurn) 
                return;

            // Check all possible moves for this piece
            ConverterUtils.ForEachSquare((targetRow, targetCol) =>
            {
                MoveData testMove = new MoveData(new Vector2Int(col, row), new Vector2Int(targetCol, targetRow), piece);

                if (ChessRules.CanPieceMoveToPosition(boardState, testMove))
                {
                    possibleMoves.Add(testMove);
                }
            });
        });

        // Return random move or null if no moves available
        if (possibleMoves.Count == 0)
            return null;

        int randomIndex = Random.Range(0, possibleMoves.Count);
        return possibleMoves[randomIndex];
    }
}
