using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BoardRenderer : MonoBehaviour
{
    [Header("Squares")]
    public GameObject whiteSquarePrefab;
    public GameObject darkSquarePrefab;
    [Header("Filters")]
    public GameObject HoverFilter;
    public GameObject CaptureFilter;
    public GameObject moveFilter;
    private GameObject[] ActiveLastMoveIndicator;
    private List<GameObject> ActivePossibleMoveIndicators;
    private GameObject Parent;

    void OnEnable()
    {
        Parent = transform.Find("BoardParent")?.gameObject;
        ScreenUtils.RemoveChildren(Parent);
        ActivePossibleMoveIndicators = new();
    }

    public void InitializeSquares()
    {
        ConverterUtils.ForEachSquare((row, col) =>
        {
            GameObject square = (row + col) % 2 != 0 ? whiteSquarePrefab : darkSquarePrefab;
            square = Instantiate(square, Parent.transform);
            square.transform.localPosition = ConverterUtils.GetRealPos(new Vector2Int(row, col), 0);
        });
    }

    public void DrawLastMoveIndicators(MoveData moveData)
    {
        if(Parent == null) Debug.Log("God dammit");
        // clear previous active filters
        if (ActiveLastMoveIndicator != null)
            foreach (GameObject filter in ActiveLastMoveIndicator)
                if (filter != null) Destroy(filter);

        ActiveLastMoveIndicator = new GameObject[2];
        ActiveLastMoveIndicator[0] = Instantiate(moveFilter, ConverterUtils.GetRealPos(moveData.pos, 1), Quaternion.identity, Parent.transform);

        GameObject targetFilter = moveData.MoveSpeciality == MoveSpeciality.isCapture ? CaptureFilter : moveFilter;
        ActiveLastMoveIndicator[1] = Instantiate(targetFilter, ConverterUtils.GetRealPos(moveData.newPos, 1), Quaternion.identity, Parent.transform);
    }

    public void DrawPossibleMoves(int[,] possibleMoves)
    {
        ClearPossibleMovesIndicator();
        ConverterUtils.ForEachSquare((row, col) =>
        {
            if (possibleMoves[row, col] == 1)
            {
                GameObject indicator = Instantiate(HoverFilter,
                    ConverterUtils.GetRealPos(new Vector2Int(col, row), 1.5f),
                    Quaternion.identity,
                    Parent.transform);
                ActivePossibleMoveIndicators.Add(indicator);
            }
                
        });
    }
    
    public void ClearPossibleMovesIndicator()
    {
        if (ActivePossibleMoveIndicators != null)
        {
            foreach (GameObject indicator in ActivePossibleMoveIndicators)
            {
                if (indicator != null) Destroy(indicator);
            }
            ActivePossibleMoveIndicators.Clear();
        }
    }    
}
