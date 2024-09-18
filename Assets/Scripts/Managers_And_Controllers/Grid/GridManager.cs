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

    // Genera la cuadr�cula de bloques
    void GenerateGrid()
    {
        gridArray = new GameObject[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight / 2; y++)
            {
                Vector3 spawnPosition = new Vector3(x, y, 0);
                GameObject block = Instantiate(GetRandomBlock(x, y), spawnPosition, Quaternion.identity);
                block.GetComponent<BlockController>().SetGridPosition(new Vector2Int(x, y));
                gridArray[x, y] = block;
            }
        }
    }

    // Obtiene un bloque aleatorio que evita generar 3 o m�s piezas id�nticas juntas
    GameObject GetRandomBlock(int x, int y)
    {
        List<GameObject> possibleBlocks = new List<GameObject>(blockPrefabs);

        // Asegurarse de que no se generan 3 o m�s bloques iguales en fila
        if (x > 1 && gridArray[x - 1, y] != null && gridArray[x - 2, y] != null)
        {
            Sprite prevBlock1Sprite = gridArray[x - 1, y].GetComponent<SpriteRenderer>().sprite;
            Sprite prevBlock2Sprite = gridArray[x - 2, y].GetComponent<SpriteRenderer>().sprite;

            if (prevBlock1Sprite == prevBlock2Sprite)
            {
                possibleBlocks.RemoveAll(block => block.GetComponent<SpriteRenderer>().sprite == prevBlock1Sprite);
            }
        }

        // Asegurarse de que no se generan 3 o m�s bloques iguales en columna
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

    // Manejador de entrada de rat�n para clics y swipe
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
            /*else
            {
                direction = swipe.y > 0 ? Vector2Int.up : Vector2Int.down;
            }*/

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

                // Empezamos la animaci�n de intercambio
                StartCoroutine(SmoothSwap(block1, block2, block1Pos, block2Pos));
            }
        }
    }

    // Corrutina para intercambiar bloques suavemente
    IEnumerator SmoothSwap(GameObject block1, GameObject block2, Vector2Int block1Pos, Vector2Int block2Pos)
    {
        float elapsedTime = 0f;
        float duration = 0.15f;  // Duraci�n de la animaci�n

        Vector3 startPos1 = block1.transform.position;
        Vector3 startPos2 = block2.transform.position;

        // Animaci�n de intercambio suave
        while (elapsedTime < duration)
        {
            if (block1 != null && block2 != null)  // Verificamos que ambos bloques a�n existen
            {
                block1.transform.position = Vector3.Lerp(startPos1, startPos2, elapsedTime / duration);
                block2.transform.position = Vector3.Lerp(startPos2, startPos1, elapsedTime / duration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de que ambos bloques lleguen a la posici�n final
        if (block1 != null) block1.transform.position = startPos2;
        if (block2 != null) block2.transform.position = startPos1;

        // Actualizar las posiciones en la cuadr�cula
        if (block1 != null) block1.GetComponent<BlockController>().SetGridPosition(block2Pos);
        if (block2 != null) block2.GetComponent<BlockController>().SetGridPosition(block1Pos);

        // Verificar si hay coincidencias despu�s del intercambio
        StartCoroutine(CheckMatchesAndDestroy(block1Pos, block2Pos));
    }

    // Verifica si hay coincidencias y destruye bloques si las hay
    IEnumerator CheckMatchesAndDestroy(Vector2Int pos1, Vector2Int pos2)
    {
        yield return new WaitForSeconds(0.1f);

        // Verificar si a�n existen los bloques antes de intentar operar con ellos
        GameObject block1 = gridArray[pos1.x, pos1.y];
        GameObject block2 = gridArray[pos2.x, pos2.y];

        CheckMatches(pos1);
        CheckMatches(pos2);

        yield return new WaitForSeconds(0.2f);  // Esperamos un poco para que se vea mejor

        // Verificar si a�n existen los bloques antes de intentar operar con ellos
        if (block1 != null) block1.GetComponent<BlockController>().SetGridPosition(pos1);
        if (block2 != null) block2.GetComponent<BlockController>().SetGridPosition(pos2);

        HandleBlockFall();  // Hacer que los bloques caigan si hay espacio vac�o debajo
    }

    // Hacer que los bloques caigan si hay espacio vac�o debajo
    void HandleBlockFall()
    {
        bool hasFallingBlocks = false;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 1; y < gridHeight; y++)  // Comenzamos en y = 1 ya que no tiene nada debajo
            {
                if (gridArray[x, y] != null)
                {
                    int dropDistance = 0;

                    // Contamos cu�ntos espacios vac�os hay debajo
                    while (y - dropDistance - 1 >= 0 && gridArray[x, y - dropDistance - 1] == null)
                    {
                        dropDistance++;
                    }

                    // Si hay un espacio vac�o, hacemos que el bloque caiga
                    if (dropDistance > 0)
                    {
                        gridArray[x, y - dropDistance] = gridArray[x, y];
                        gridArray[x, y] = null;

                        Vector3 targetPosition = new Vector3(x, y - dropDistance, 0);
                        StartCoroutine(SmoothMove(gridArray[x, y - dropDistance], targetPosition));
                        hasFallingBlocks = true;  // Indicamos que hubo bloques que cayeron
                    }
                }
            }
        }

        // Si hubo bloques que cayeron, verificamos nuevas coincidencias despu�s de que caigan
        if (hasFallingBlocks)
        {
            StartCoroutine(CheckForNewMatchesAfterFall());
        }
    }

    IEnumerator CheckForNewMatchesAfterFall()
    {
        // Esperamos a que termine el movimiento de ca�da
        yield return new WaitForSeconds(0.2f);

        bool foundNewMatches = false;

        // Recorremos toda la cuadr�cula para verificar coincidencias despu�s de que los bloques cayeron
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridArray[x, y] != null)
                {
                    if (CheckMatches(new Vector2Int(x, y)))
                    {
                        foundNewMatches = true;
                    }
                }
            }
        }

        // Si encontramos nuevas coincidencias, hacemos que los bloques caigan de nuevo
        if (foundNewMatches)
        {
            HandleBlockFall();
        }
    }


    // Corrutina para mover bloques suavemente
    IEnumerator SmoothMove(GameObject block, Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        float duration = 0.15f;  // Duraci�n de la animaci�n de ca�da

        Vector3 startPosition = block.transform.position;

        while (elapsedTime < duration)
        {
            if (block != null)  // Verificamos que el bloque a�n existe
            {
                block.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de que el bloque llegue a la posici�n final
        if (block != null) block.transform.position = targetPosition;
    }

    // Verifica si una posici�n es v�lida dentro de la cuadr�cula
    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    // Verificar coincidencias (matches) para destruir bloques
    bool CheckMatches(Vector2Int pos)
    {
        GameObject block = gridArray[pos.x, pos.y];
        if (block == null) return false;

        SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();
        List<Vector2Int> horizontalMatches = new List<Vector2Int>();
        List<Vector2Int> verticalMatches = new List<Vector2Int>();

        // Buscar coincidencias horizontales
        horizontalMatches.Add(pos);
        for (int i = pos.x - 1; i >= 0; i--)
        {
            if (gridArray[i, pos.y] != null && gridArray[i, pos.y].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
            {
                horizontalMatches.Add(new Vector2Int(i, pos.y));
            }
            else
            {
                break;
            }
        }
        for (int i = pos.x + 1; i < gridWidth; i++)
        {
            if (gridArray[i, pos.y] != null && gridArray[i, pos.y].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
            {
                horizontalMatches.Add(new Vector2Int(i, pos.y));
            }
            else
            {
                break;
            }
        }

        // Buscar coincidencias verticales
        verticalMatches.Add(pos);
        for (int i = pos.y - 1; i >= 0; i--)
        {
            if (gridArray[pos.x, i] != null && gridArray[pos.x, i].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
            {
                verticalMatches.Add(new Vector2Int(pos.x, i));
            }
            else
            {
                break;
            }
        }
        for (int i = pos.y + 1; i < gridHeight; i++)
        {
            if (gridArray[pos.x, i] != null && gridArray[pos.x, i].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
            {
                verticalMatches.Add(new Vector2Int(pos.x, i));
            }
            else
            {
                break;
            }
        }

        bool matchFound = false;

        // Si hay 3 o m�s coincidencias horizontales o verticales, destruimos los bloques
        if (horizontalMatches.Count >= 3)
        {
            foreach (Vector2Int match in horizontalMatches)
            {
                Destroy(gridArray[match.x, match.y]);
                gridArray[match.x, match.y] = null;
            }
            matchFound = true;
        }

        if (verticalMatches.Count >= 3)
        {
            foreach (Vector2Int match in verticalMatches)
            {
                Destroy(gridArray[match.x, match.y]);
                gridArray[match.x, match.y] = null;
            }
            matchFound = true;
        }

        return matchFound;
    }
}
