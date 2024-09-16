using UnityEngine;

public class BlockController : MonoBehaviour
{
    private Vector2Int gridPosition;

    public void SetGridPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
}
