using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[ExecuteAlways]
[RequireComponent(typeof(ServerScript))]
public class BoardLogic : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);
    [Header("Squares")]
    public GameObject Squares;
    public GameObject Pieces;
    public GameObject HoverFilter;
    public GameObject CaptureFilter;
    public GameObject whiteSquarePrefab;
    public GameObject darkSquarePrefab;
    public GameObject moveFilter;
    private GameObject[] activeFilters;

    [Header("Pieces")]
    public GameObject[] BlackPieces;
    public GameObject[] WhitePieces;

    [Header("UI")]
    public TextMeshProUGUI turnText;
    public GameObject WhitePawnPromotionDialog;
    public GameObject BlackPawnPromotionDialog;
    public GameObject UICanvas;
    public TextMeshProUGUI CheckLabel;
    public EnemyType enemyType = EnemyType.SingleScreen;
    public AudioClip capture;
    public AudioClip move;
    public AudioClip vineThud;
    public AudioClip yipee;
    public AudioSource audioSource;
    public bool boardFlipped;

    private BoardState State;
    private ServerScript serverScript;

    private float BOARD_OFFSET = 3.5f;
    private GameObject hoveringPiece = null;


    // Start is called before the first frame update
    void Start()
    {
        activeFilters = new GameObject[2];
        DrawBoardSquares();
        State = new(WhitePieces, BlackPieces);
        DrawPieces();
        serverScript = GetComponent<ServerScript>();

        Debug.Log(State.ToJson());


        Debug.Log(BoardState.FromJson(State.ToJson(), WhitePieces, BlackPieces).ToJson());
    }

    void DrawBoardSquares()
    {
        for (int i = Squares.transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(Squares.transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(Squares.transform.GetChild(i).gameObject);
            }
        }

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject square = (row + col) % 2 != 0 ? whiteSquarePrefab : darkSquarePrefab;
                square = Instantiate(square, Vector3.zero, Quaternion.identity, Squares.transform);
                square.transform.localPosition = new Vector3(drawY(row - BOARD_OFFSET), col - BOARD_OFFSET, 0);
            }
        }
    }

    void DrawPieces()
    {
        for (int i = Pieces.transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(Pieces.transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(Pieces.transform.GetChild(i).gameObject);
            }
        }

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (State.getPos(col, row) == null) continue;
                GameObject piece = Instantiate(State.getPos(col, row), Vector3.zero, Quaternion.identity, Pieces.transform);
                piece.transform.localPosition = new Vector3(col - BOARD_OFFSET, drawY(row - BOARD_OFFSET), 0);
                piece.transform.localScale = Vector3.one * 0.75f;
            }
        }
    }
    public enum EnemyType
    {
        SingleScreen,
        RandomBot,
        Multiplayer
    }

    enum GameStage
    {
        Default,
        Promotion,
        GameOver
    };

    GameStage GameMode = GameStage.Default;
    private bool whiteTurn = true;
    private bool isChecked = false;
    private bool botThinking = false; // Add this flag
    void Update()
    {
        if (!Application.isPlaying) return;
        switch (GameMode)
        {
            case GameStage.Default:
                HandleInput();
                break;
            case GameStage.Promotion:
                HandlePawnPromotion();
                break;
            case GameStage.GameOver:
                break;
        }
    }

    void HandleInput()
    {
        switch (enemyType)
        {
            case EnemyType.SingleScreen:
                ScreenInputHandeler();
                return;
            case EnemyType.RandomBot:
                if (whiteTurn != boardFlipped)
                {
                    ScreenInputHandeler();
                }
                else if (!botThinking) // Only start thinking if not already thinking
                {
                    botThinking = true;
                    StartCoroutine(MakeRandomMoveWithDelay());
                }
                return;
            case EnemyType.Multiplayer:
                if (whiteTurn != boardFlipped)
                {
                    ScreenInputHandeler();
                }
                else if (!botThinking)
                {
                    botThinking = true;
                    serverScript.SendMove(State.ToJson());
                    Debug.Log("move send");
                }
                return;
        }
    }

    System.Collections.IEnumerator MakeRandomMoveWithDelay()
    {
        yield return _waitForSeconds0_5;
        MakeRandomMove();
        botThinking = false; // Reset the flag after making the move
    }

    void ScreenInputHandeler()
    {
        if (Input.GetMouseButtonDown(0) && !mouseDown)
            SelectHoveringPiece();
        else if (Input.GetMouseButtonUp(0) && mouseDown)
            PlaceHoveredPiece();
        else if (mouseDown && hoveringPiece != null)
            UpdateHoveredPiecePos();
    }

    void MakeRandomMove()
    {
        List<(Vector2Int from, Vector2Int to, GameObject piece)> possibleMoves = new();
        GameObject[] currentPlayerPieces = whiteTurn ? WhitePieces : BlackPieces;

        // Find all possible moves for the current player
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = State.getPos(col, row);
                if (piece == null || !currentPlayerPieces.Contains(piece)) continue;

                // Check all possible moves for this piece
                for (int targetRow = 0; targetRow < 8; targetRow++)
                {
                    for (int targetCol = 0; targetCol < 8; targetCol++)
                    {
                        if (State.CanPieceMoveToPosition(piece, new Vector2Int(col, row), new Vector2Int(targetCol, targetRow), whiteTurn))
                        {
                            // ALWAYS check if this move would leave the king in check (not just when already in check)
                            BoardState testState = new BoardState(State);
                            testState.setPos(col, row, null);
                            testState.setPos(targetCol, targetRow, piece);
                            if (!testState.IsChecked(whiteTurn))
                            {
                                possibleMoves.Add((new Vector2Int(col, row), new Vector2Int(targetCol, targetRow), piece));
                            }
                        }
                    }
                }
            }
        }

        // If no moves available, it's checkmate or stalemate
        if (possibleMoves.Count == 0)
        {
            if (isChecked)
            {
                GameMode = GameStage.GameOver;
                CheckLabel.text = (whiteTurn ? "Black" : "White") + " wins Checkmate";
                turnText.text = "";
            }
            else
            {
                HandleStalemate();
            }
            return;
        }

        // Pick a random move
        int randomIndex = UnityEngine.Random.Range(0, possibleMoves.Count);
        var randomMove = possibleMoves[randomIndex];

        // Execute the move
        State.setPos(randomMove.from.x, randomMove.from.y, null); // Remove piece from original position
        bool isCapture = State.getPos(randomMove.to.x, randomMove.to.y) != null;
        State.setPos(randomMove.to.x, randomMove.to.y, randomMove.piece); // Place piece at new position
        DrawLastMove(randomMove.from, randomMove.to, isCapture);

        // Check for pawn promotion
        if (randomMove.piece == BlackPieces[5] && randomMove.to.y == 0 || randomMove.piece == WhitePieces[5] && randomMove.to.y == 7)
        {
            // Auto-promote to queen for AI
            GameObject queenPiece = whiteTurn ? WhitePieces[3] : BlackPieces[3];
            State.setPos(randomMove.to.x, randomMove.to.y, queenPiece);
        }
        NextTurn();
    }

    private Vector2Int pos;
    private bool mouseDown = false;
    private GameObject hoverPrefab;
    void SelectHoveringPiece()
    {
        pos = GetclickCords();
        if (pos.x < 0 || pos.x > 7 || pos.y < 0 || pos.y > 7) return;
        hoverPrefab = State.getPos(pos.x, pos.y);
        if (hoverPrefab == null || whiteTurn != WhitePieces.Contains(hoverPrefab)) return;
        mouseDown = true;
        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hoveringPiece = Instantiate(hoverPrefab, new Vector3(mousepos.x, mousepos.y, this.Pieces.transform.position.z - 1), Quaternion.identity);
        hoveringPiece.transform.localScale = Vector3.one * 0.9f;
        State.setPos(pos.x, pos.y, null);
        DrawPieces();
        DrawPossibleMoves(hoverPrefab, pos);
    }

    void PlaceHoveredPiece()
    {
        Vector2Int newPos = GetclickCords();
        BoardState testState = new BoardState(State);
        testState.setPos(pos.x, pos.y, null);
        testState.setPos(newPos.x, newPos.y, hoverPrefab);
        if (!State.CanPieceMoveToPosition(hoverPrefab, pos, newPos, whiteTurn) || testState.IsChecked(whiteTurn))
        {
            Destroy(hoveringPiece);
            mouseDown = false;
            State.setPos(pos.x, pos.y, hoverPrefab);
            DrawPieces();
            return;
        }


        bool isCapture = State.MovePiece(newPos, pos, hoverPrefab);
        DrawLastMove(pos, newPos, isCapture);
        // check for pawn promotion
        if (hoverPrefab == BlackPieces[5] && newPos.y == 0 || hoverPrefab == WhitePieces[5] && newPos.y == 7)
        {
            GameMode = GameStage.Promotion;
            screenDrawn = false;
            pos = newPos;
        }
        Destroy(hoveringPiece);
        mouseDown = false;
        NextTurn();
    }

    void HandleCheck()
    {
        isChecked = State.IsChecked(whiteTurn);
        CheckLabel.alpha = 0;
        if (isChecked)
        {
            CheckLabel.alpha = 100;
            // Check for checkmate
            if (IsCheckmate())
            {
                GameMode = GameStage.GameOver;
                CheckLabel.text = (whiteTurn ? "Black" : "White") + " wins Checkmate";
                turnText.text = "";
            }
        }
    }

    Vector2Int GetclickCords()
    {
        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousepos.y = drawY(mousepos.y);
        mousepos += new Vector3(4, 4, 0);
        return new Vector2Int(Mathf.FloorToInt(mousepos.x), Mathf.FloorToInt(mousepos.y));
    }

    void UpdateHoveredPiecePos()
    {
        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hoveringPiece.transform.position = new Vector3(mousepos.x, mousepos.y, this.Pieces.transform.position.z - 1);
        hoveringPiece.transform.localScale = Vector3.one * 0.9f;
    }

    void NextTurn()
    {
        whiteTurn = !whiteTurn;
        UpdateTurnText(whiteTurn);
        isChecked = State.IsChecked(whiteTurn);
        HandleCheck();
        DrawPieces();

        // stalemate
        if (State.IsStalemate(whiteTurn))
        {
            HandleStalemate();
        }
    }

    void HandleStalemate()
    {
        GameMode = GameStage.GameOver;
        CheckLabel.text = "Its a draw!";
        turnText.text = "";
        CheckLabel.alpha = 100;
        audioSource.clip = vineThud;
        audioSource.Play();
    }

    void UpdateTurnText(bool whiteTurn)
    {
        string text = whiteTurn ? "White's Turn" : "Black's turn";
        turnText.SetText(text);
        turnText.color = whiteTurn ? Color.white : Color.black;
    }

    bool screenDrawn = false;
    void HandlePawnPromotion()
    {
        if (screenDrawn) return;

        GameObject promotionDialog = Instantiate(!whiteTurn ? WhitePawnPromotionDialog : BlackPawnPromotionDialog, Vector3.zero, Quaternion.identity, this.UICanvas.transform);
        promotionDialog.transform.localPosition = Vector3.zero;
        screenDrawn = true;

        Button[] buttons = promotionDialog.GetComponentsInChildren<Button>();

        bool isWhite = !whiteTurn;

        buttons[0].onClick.AddListener(() => PromotePawn(isWhite ? WhitePieces[3] : BlackPieces[3], promotionDialog));
        buttons[1].onClick.AddListener(() => PromotePawn(isWhite ? WhitePieces[1] : BlackPieces[1], promotionDialog));
        buttons[2].onClick.AddListener(() => PromotePawn(isWhite ? WhitePieces[0] : BlackPieces[0], promotionDialog));
        buttons[3].onClick.AddListener(() => PromotePawn(isWhite ? WhitePieces[2] : BlackPieces[2], promotionDialog));
    }

    void PromotePawn(GameObject chosenPiece, GameObject promotionDialog)
    {
        GameMode = GameStage.Default;
        Destroy(promotionDialog);
        State.setPos(pos.x, pos.y, chosenPiece);
        DrawPieces();
        HandleCheck();
    }

    void DrawPossibleMoves(GameObject hoverPrefab, Vector2Int pos)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (State.CanPieceMoveToPosition(hoverPrefab, pos, new Vector2Int(col, row), whiteTurn))
                {
                    BoardState testState = new BoardState(State);
                    testState.setPos(pos.x, pos.y, null);
                    testState.setPos(col, row, hoverPrefab);
                    if (!testState.IsChecked(whiteTurn))
                    {
                        var test = Instantiate(HoverFilter, Vector3.zero, Quaternion.identity, this.Pieces.transform);
                        test.transform.localPosition = new Vector3(col - BOARD_OFFSET, drawY(row - BOARD_OFFSET), 0.5f);
                    }
                }
            }
        }
    }

    void DrawLastMove(Vector2Int pos, Vector2Int newPos, bool isCapture)
    {
        for (int i = 0; i < activeFilters.Length; i++)
        {
            Destroy(activeFilters[i]);
        }

        activeFilters[0] = Instantiate(moveFilter, new Vector3(pos.x - BOARD_OFFSET, drawY(pos.y - BOARD_OFFSET), -0.3f), Quaternion.identity, Squares.transform);

        if (isCapture)
        {
            activeFilters[1] = Instantiate(CaptureFilter, new Vector3(newPos.x - BOARD_OFFSET, drawY(newPos.y - BOARD_OFFSET), -0.3f), Quaternion.identity, Squares.transform);
        }
        else
        {
            activeFilters[1] = Instantiate(moveFilter, new Vector3(newPos.x - BOARD_OFFSET, drawY(newPos.y - BOARD_OFFSET), -0.3f), Quaternion.identity, Squares.transform);
        }


        audioSource.clip = move;
        if (isCapture) audioSource.clip = capture;
        audioSource.Play();
    }

    bool IsCheckmate()
    {
        // If not in check, it's not checkmate
        if (!isChecked) return false;

        // Check if any piece of the current player can make a legal move
        GameObject[] currentPlayerPieces = whiteTurn ? WhitePieces : BlackPieces;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = State.getPos(col, row);
                if (piece == null || !currentPlayerPieces.Contains(piece)) continue;

                // Check all possible moves for this piece
                for (int targetRow = 0; targetRow < 8; targetRow++)
                {
                    for (int targetCol = 0; targetCol < 8; targetCol++)
                    {
                        if (State.CanPieceMoveToPosition(piece, new Vector2Int(col, row), new Vector2Int(targetCol, targetRow), whiteTurn))
                        {
                            // Test if this move would get out of check
                            BoardState testState = new BoardState(State);
                            testState.setPos(col, row, null);
                            testState.setPos(targetCol, targetRow, piece);
                            if (!testState.IsChecked(whiteTurn))
                            {
                                return false; // Found a legal move, not checkmate
                            }
                        }
                    }
                }
            }
        }

        if (boardFlipped == whiteTurn)
        {
            audioSource.clip = yipee;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = vineThud;
            audioSource.Play();
        }

        return true; // No legal moves found, it's checkmate
    }

    float drawY(float y)
    {
        return boardFlipped ? -y : y;
    }
}