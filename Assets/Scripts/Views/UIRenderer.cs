using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRenderer : MonoBehaviour
{
    public TextMeshProUGUI TurnText;
    public TextMeshProUGUI GameStateLabel;
    public GameObject WhitePawnPromotionDialog;
    public GameObject BlackPawnPromotionDialog;
    public GameObject UICanvas;
    public System.Action<int, GameObject> OnPromotionChoice;

    public void SetTurnText(bool isWhite)
    {
        string text = isWhite ? "White's Turn" : "Black's Turn";
        TurnText.SetText(text);
        TurnText.color = isWhite ? Color.white : Color.black;
    }


    public void SetGameStateText(MoveData moveData)
    {
        string text = moveData.gameState switch
        {
            GameState.Default => moveData.MoveSpeciality == MoveSpeciality.isCheck ? "Check!" : "",
            GameState.Stalemate => "Stalemate!",
            GameState.Draw => "Draw!",
            GameState.WhiteCheckmate => "White won by checkmate!",
            GameState.BlackCheckmate => "Black won by checkmate!",
            GameState.WhiteResigned => "White resigned the match",
            GameState.BlackResigned => "Black resigned the match",
            _ => throw new System.NotImplementedException()
        };
        GameStateLabel.SetText(text);
    }

    public GameObject RenderPromotionDialog(bool isWhite)
    {
        GameObject promotionDialog = Instantiate(
            isWhite ? WhitePawnPromotionDialog : BlackPawnPromotionDialog, 
            Vector3.zero, 
            Quaternion.identity, 
            UICanvas.transform
        );
        promotionDialog.transform.localPosition = Vector3.zero;

        Button[] buttons = promotionDialog.GetComponentsInChildren<Button>();
        
        // Map buttons to piece types: Queen, Rook, Bishop, Knight
        int[] pieceTypes = isWhite ? new[] { 4, 1, 3, 2 } : new[] { 10, 7, 9, 8 };
        
        for (int i = 0; i < buttons.Length && i < pieceTypes.Length; i++)
        {
            int promotionPieceType = pieceTypes[i];
            buttons[i].onClick.AddListener(() => OnPromotionChoice?.Invoke(promotionPieceType, promotionDialog));
        }

        return promotionDialog;
    }
    
}
