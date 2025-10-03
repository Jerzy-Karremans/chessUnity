using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PieceRenderer : MonoBehaviour
{
    public GameObject[] WhitePieces;
    public GameObject[] BlackPieces;
    private GameObject Parent;
    private Dictionary<Vector2Int, GameObject> PieceObjects = new();
    public GameObject HoveringPiece;

    void OnEnable()
    {
        Parent = transform.Find("PiecesParent")?.gameObject;
        ScreenUtils.RemoveChildren(Parent);
    }

    public void DrawPieces(int[,] board)
    {
        ClearPieces();

        ConverterUtils.ForEachSquare((row, col) =>
        {
            int piece = board[row, col];

            if (piece != 0)
            {
                GameObject piecePrefab = piece <= 6 ? WhitePieces[piece - 1] : BlackPieces[piece - 7];
                var pieceGO = Instantiate(piecePrefab,
                    ConverterUtils.GetRealPos(new Vector2Int(col, row), 2),
                    Quaternion.identity,
                    Parent.transform);

                pieceGO.transform.localScale = Vector3.one * 0.75f;
                PieceObjects[new Vector2Int(col, row)] = pieceGO;
            }
        });
    }

    private void ClearPieces()
    {
        if (Parent == null) return;

        for (int i = Parent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = Parent.transform.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        PieceObjects.Clear();
    }

    public void HandleMouseDrag()
    {
        if (HoveringPiece == null) return;
        HoveringPiece.transform.position = ScreenUtils.GetMousePos();
    }


    public GameObject GetPieceAt(Vector2Int pos)
    {
        return PieceObjects.TryGetValue(pos, out GameObject piece) ? piece : null;
    }
}
