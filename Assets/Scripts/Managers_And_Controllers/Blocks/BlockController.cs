using UnityEngine;

public class BlockController : MonoBehaviour
{
    private GridManager gridManager;
    private Vector2Int gridPosition; // Posición en la cuadrícula
    private Vector2 startPos, endPos;
    private float swipeDistanceThreshold = 0.5f;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    // Asignar posición en la cuadrícula
    public void SetGridPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
    }

    void OnMouseDown()
    {
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseUp()
    {
        endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        HandleSwipe();
    }

    void HandleSwipe()
    {
        Vector2 swipe = endPos - startPos;

        if (swipe.magnitude >= swipeDistanceThreshold)
        {
            // Swipe horizontal
            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                if (swipe.x > 0)
                {
                    gridManager.SwapBlocks(gridPosition, Vector2Int.right);
                }
                else
                {
                    gridManager.SwapBlocks(gridPosition, Vector2Int.left);
                }
            }
            // Swipe vertical
            else
            {
                if (swipe.y > 0)
                {
                    gridManager.SwapBlocks(gridPosition, Vector2Int.up);
                }
                else
                {
                    gridManager.SwapBlocks(gridPosition, Vector2Int.down);
                }
            }
        }
    }
}
