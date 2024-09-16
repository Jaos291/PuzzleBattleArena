using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 10;
    public GameObject[] blockPrefabs;
    public float swipeDistanceThreshold = 0.5f;

    private GameObject[,] gridArray;
    private GameObject selectedBlock = null;
    private Vector2 startPos, endPos;

    void Start()
    {
        GenerateGrid();
    }

    void Update()
    {
        HandleMouseInput();
    }

    // Genera la cuadrícula de bloques
    void GenerateGrid()
    {
        gridArray = new GameObject[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 spawnPosition = new Vector3(x, y, 0);
                GameObject block = Instantiate(GetRandomBlock(x, y), spawnPosition, Quaternion.identity);
                block.GetComponent<BlockController>().SetGridPosition(new Vector2Int(x, y));
                gridArray[x, y] = block;
            }
        }
    }

    // Obtiene un bloque aleatorio que evita generar 3 o más piezas idénticas juntas
    GameObject GetRandomBlock(int x, int y)
    {
        List<GameObject> possibleBlocks = new List<GameObject>(blockPrefabs);

        // Asegurarse de que no se generan 3 o más bloques iguales en fila
        if (x > 1 && gridArray[x - 1, y] != null && gridArray[x - 2, y] != null)
        {
            Sprite prevBlock1Sprite = gridArray[x - 1, y].GetComponent<SpriteRenderer>().sprite;
            Sprite prevBlock2Sprite = gridArray[x - 2, y].GetComponent<SpriteRenderer>().sprite;

            if (prevBlock1Sprite == prevBlock2Sprite)
            {
                possibleBlocks.RemoveAll(block => block.GetComponent<SpriteRenderer>().sprite == prevBlock1Sprite);
            }
        }

        // Asegurarse de que no se generan 3 o más bloques iguales en columna
        if (y > 1 && gridArray[x, y - 1] != null && gridArray[x, y - 2] != null)
        {
            Sprite prevBlock1Sprite = gridArray[x, y - 1].GetComponent<SpriteRenderer>().sprite;
            Sprite prevBlock2Sprite = gridArray[x, y - 2].GetComponent<SpriteRenderer>().sprite;

            if (prevBlock1Sprite == prevBlock2Sprite)
            {
                possibleBlocks.RemoveAll(block => block.GetComponent<SpriteRenderer>().sprite == prevBlock1Sprite);
            }
        }

        // Devuelve un bloque aleatorio de los posibles restantes
        return possibleBlocks[Random.Range(0, possibleBlocks.Count)];
    }

    // Manejador de entrada de ratón para clics y swipe
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject clickedBlock = hit.collider.gameObject;

                if (selectedBlock == null)
                {
                    // Si no hay un bloque seleccionado, seleccionamos el bloque clicado
                    selectedBlock = clickedBlock;
                }
                else
                {
                    // Intentamos intercambiar si ambos bloques son adyacentes
                    Vector2Int selectedPos = selectedBlock.GetComponent<BlockController>().GetGridPosition();
                    Vector2Int clickedPos = clickedBlock.GetComponent<BlockController>().GetGridPosition();

                    if (AreBlocksAdjacent(selectedPos, clickedPos))
                    {
                        SwapBlocks(selectedPos, clickedPos);
                    }

                    // Deseleccionamos el bloque
                    selectedBlock = null;
                }
            }
        }

        // Manejamos el swipe
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && selectedBlock != null)
        {
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleSwipe();
            selectedBlock = null;
        }
    }

    // Verifica si dos bloques son adyacentes
    bool AreBlocksAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        return (Mathf.Abs(pos1.x - pos2.x) == 1 && pos1.y == pos2.y) || (Mathf.Abs(pos1.y - pos2.y) == 1 && pos1.x == pos2.x);
    }

    // Maneja el swipe entre bloques
    void HandleSwipe()
    {
        Vector2 swipe = endPos - startPos;

        if (swipe.magnitude >= swipeDistanceThreshold)
        {
            Vector2Int direction = Vector2Int.zero;

            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                direction = swipe.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                direction = swipe.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            Vector2Int currentBlockPos = selectedBlock.GetComponent<BlockController>().GetGridPosition();
            Vector2Int targetBlockPos = currentBlockPos + direction;

            if (IsValidPosition(targetBlockPos))
            {
                SwapBlocks(currentBlockPos, targetBlockPos);
            }
        }
    }

    // Intercambia dos bloques
    void SwapBlocks(Vector2Int block1Pos, Vector2Int block2Pos)
    {
        if (IsValidPosition(block1Pos) && IsValidPosition(block2Pos))
        {
            GameObject block1 = gridArray[block1Pos.x, block1Pos.y];
            GameObject block2 = gridArray[block2Pos.x, block2Pos.y];

            if (block1 != null && block2 != null)
            {
                // Intercambiar en el array
                gridArray[block1Pos.x, block1Pos.y] = block2;
                gridArray[block2Pos.x, block2Pos.y] = block1;

                // Intercambiar visualmente las posiciones
                Vector3 tempPosition = block1.transform.position;
                block1.transform.position = block2.transform.position;
                block2.transform.position = tempPosition;

                // Actualizar las posiciones en la cuadrícula
                block1.GetComponent<BlockController>().SetGridPosition(block2Pos);
                block2.GetComponent<BlockController>().SetGridPosition(block1Pos);

                // Verificar si hay coincidencias después del intercambio
                StartCoroutine(CheckMatchesAndDestroy(block1Pos, block2Pos));
            }
        }
    }

    // Verifica si hay coincidencias y destruye bloques si las hay
    IEnumerator CheckMatchesAndDestroy(Vector2Int pos1, Vector2Int pos2)
    {
        yield return new WaitForSeconds(0.1f);

        CheckMatches(pos1);
        CheckMatches(pos2);
    }

    // Comprobar las coincidencias (tres o más bloques del mismo tipo en línea)
    void CheckMatches(Vector2Int blockPos)
    {
        List<GameObject> horizontalMatches = FindMatchesInDirection(blockPos, Vector2Int.left);
        horizontalMatches.AddRange(FindMatchesInDirection(blockPos, Vector2Int.right));

        List<GameObject> verticalMatches = FindMatchesInDirection(blockPos, Vector2Int.up);
        verticalMatches.AddRange(FindMatchesInDirection(blockPos, Vector2Int.down));

        if (horizontalMatches.Count >= 2)
        {
            horizontalMatches.Add(gridArray[blockPos.x, blockPos.y]);
            foreach (GameObject block in horizontalMatches)
            {
                Destroy(block);
            }
        }

        if (verticalMatches.Count >= 2)
        {
            verticalMatches.Add(gridArray[blockPos.x, blockPos.y]);
            foreach (GameObject block in verticalMatches)
            {
                Destroy(block);
            }
        }
    }

    // Encuentra bloques iguales en una dirección específica
    List<GameObject> FindMatchesInDirection(Vector2Int startPos, Vector2Int direction)
    {
        List<GameObject> matches = new List<GameObject>();
        Vector2Int checkPos = startPos + direction;

        while (IsValidPosition(checkPos))
        {
            GameObject checkBlock = gridArray[checkPos.x, checkPos.y];
            GameObject startBlock = gridArray[startPos.x, startPos.y];

            if (checkBlock != null && checkBlock.GetComponent<SpriteRenderer>().sprite == startBlock.GetComponent<SpriteRenderer>().sprite)
            {
                matches.Add(checkBlock);
            }
            else
            {
                break;
            }

            checkPos += direction;
        }

        return matches;
    }

    // Verifica si una posición es válida dentro de la cuadrícula
    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }
}
