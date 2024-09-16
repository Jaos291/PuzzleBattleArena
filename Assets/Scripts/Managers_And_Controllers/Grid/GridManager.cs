using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 12;
    public GameObject blockPrefab;
    private GameObject[,] gridArray;

    void Start()
    {
        gridArray = new GameObject[gridWidth, gridHeight];
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 position = new Vector2(x, y);
                GameObject newBlock = Instantiate(blockPrefab, position, Quaternion.identity);
                newBlock.GetComponent<BlockController>().SetGridPosition(new Vector2Int(x, y));
                gridArray[x, y] = newBlock;
            }
        }
    }

    // Función para intercambiar bloques
    public void SwapBlocks(Vector2Int currentPos, Vector2Int direction)
    {
        Vector2Int targetPos = currentPos + direction;

        // Comprobar que el intercambio es dentro de los límites de la cuadrícula
        if (targetPos.x >= 0 && targetPos.x < gridWidth && targetPos.y >= 0 && targetPos.y < gridHeight)
        {
            // Intercambiar bloques
            GameObject currentBlock = gridArray[currentPos.x, currentPos.y];
            GameObject targetBlock = gridArray[targetPos.x, targetPos.y];

            // Actualizar las posiciones de los bloques en la cuadrícula
            gridArray[currentPos.x, currentPos.y] = targetBlock;
            gridArray[targetPos.x, targetPos.y] = currentBlock;

            // Actualizar la posición visual de los bloques
            currentBlock.transform.position = new Vector2(targetPos.x, targetPos.y);
            targetBlock.transform.position = new Vector2(currentPos.x, currentPos.y);

            // Actualizar las posiciones en los bloques mismos
            currentBlock.GetComponent<BlockController>().SetGridPosition(targetPos);
            targetBlock.GetComponent<BlockController>().SetGridPosition(currentPos);

            // Después del swap, podemos chequear si hay alineaciones de bloques (match-3)
            CheckForMatches();
        }
    }

    // Verificación de alineaciones
    private void CheckForMatches()
    {
        // Recorremos la cuadrícula para ver si hay alineaciones horizontales o verticales
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject currentBlock = gridArray[x, y];
                if (currentBlock != null)
                {
                    // Chequear alineaciones horizontales
                    if (x < gridWidth - 2)
                    {
                        GameObject block1 = gridArray[x + 1, y];
                        GameObject block2 = gridArray[x + 2, y];
                        if (block1 != null && block2 != null && AreSameColor(currentBlock, block1, block2))
                        {
                            Debug.Log("¡Alineación horizontal!");
                            Destroy(currentBlock);
                            Destroy(block1);
                            Destroy(block2);
                        }
                    }

                    // Chequear alineaciones verticales
                    if (y < gridHeight - 2)
                    {
                        GameObject block1 = gridArray[x, y + 1];
                        GameObject block2 = gridArray[x, y + 2];
                        if (block1 != null && block2 != null && AreSameColor(currentBlock, block1, block2))
                        {
                            Debug.Log("¡Alineación vertical!");
                            Destroy(currentBlock);
                            Destroy(block1);
                            Destroy(block2);
                        }
                    }
                }
            }
        }
    }

    // Verificar si los bloques tienen el mismo color
    private bool AreSameColor(GameObject block1, GameObject block2, GameObject block3)
    {
        SpriteRenderer sprite1 = block1.GetComponent<SpriteRenderer>();
        SpriteRenderer sprite2 = block2.GetComponent<SpriteRenderer>();
        SpriteRenderer sprite3 = block3.GetComponent<SpriteRenderer>();

        return sprite1.sprite == sprite2.sprite && sprite2.sprite == sprite3.sprite;
    }

}
