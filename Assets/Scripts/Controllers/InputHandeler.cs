using UnityEngine;

[RequireComponent(typeof(GameController))]
[RequireComponent(typeof(PieceRenderer))]
public class InputHandeler : MonoBehaviour
{
    private GameController GameController;
    private PieceRenderer PieceRenderer;
    private bool isDragging = false;
    private Vector2Int SelectedSquare;

    void Start()
    {
        GameController = GetComponent<GameController>();
        PieceRenderer = GetComponent<PieceRenderer>();
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleMouseDown();
        else if (Input.GetMouseButtonUp(0))
            HandleMouseUp();
        else if (isDragging)
            PieceRenderer.HandleMouseDrag();
    }

    private void HandleMouseDown()
    {
        SelectedSquare = ScreenUtils.GetSquareFromMouse();
        isDragging  = GameController.OnSquareClicked(SelectedSquare);
    }

    private void HandleMouseUp()
    {
        if (isDragging)
        {
            Vector2Int targetSquare = ScreenUtils.GetSquareFromMouse();
            GameController.OnPieceMoved(SelectedSquare, targetSquare);
        }
        isDragging = false;
    }   
}
