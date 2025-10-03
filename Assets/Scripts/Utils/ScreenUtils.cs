using UnityEngine;

public class ScreenUtils : MonoBehaviour
{
    public static void RemoveChildren(GameObject parent)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (Application.isPlaying)
                Destroy(parent.transform.GetChild(i).gameObject);
        }
    }

    public static Vector2Int GetSquareFromMouse()
    {
        return ConverterUtils.GetGamePos(GetMousePos());
    }

    public static Vector3 GetMousePos()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = -3;
        return worldPos;
    }
}
