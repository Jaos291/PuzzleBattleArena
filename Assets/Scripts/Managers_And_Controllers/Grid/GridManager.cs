using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 10;
    public GameObject[] blockPrefabs;
    public float swipeDistanceThreshold = 0.5f;

    private GameObject[,] gridArray;
    private GameObject selectedBlock = null;
    private Vector2 startPos, endPos;

    public float riseSpeed = 0.125f; // Velocidad a la que sube el grid (0.125 unidades por segundo)
    private float gridOffset = 0f;   // Desplazamiento actual de la cuadrícula
    public int maxVisibleRows = 10;  // Número máximo de filas visibles en pantalla
    public float fallDelay; // Retraso antes de que los bloques comiencen a caer
    public float cubesWaitTime; // Retraso entre cada figura para desaparecer
    public float fallSpeed;

    private float _riseSpeedHolder;


    void Start()
    {
        _riseSpeedHolder = riseSpeed;
        GenerateGrid();
    }

    void Update()
    {
        HandleMouseInput();

        // Subir el grid a una velocidad constante
        if (!IsGridAtTop())
        {
            gridOffset += riseSpeed * Time.deltaTime;

            // Si el offset llega a 1, entonces hemos subido una unidad completa y es momento de añadir una nueva línea
            if (gridOffset >= 1f)
            {
                gridOffset = 0f;
                AddNewLine();
            }

            // Actualizar la posición de los bloques
            UpdateBlockPositions();
        }
    }

    // Verifica si el grid ha llegado al tope superior
    bool IsGridAtTop()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (gridArray[x, gridHeight - 1] != null)
            {
                return true; // Si alguna celda en la fila superior está ocupada, llegamos al tope
            }
        }
        return false;
    }

    // Actualiza la posición de todos los bloques según el offset del grid
    void UpdateBlockPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridArray[x, y] != null)
                {
                    Vector3 newPosition = new Vector3(x, y + gridOffset, 0);
                    gridArray[x, y].transform.position = newPosition;

                    // Si el bloque está en la fila 1 o más arriba, hacerlo usable
                    if (y + gridOffset >= 1 && !gridArray[x, y].GetComponent<BlockController>().IsUsable())
                    {
                        gridArray[x, y].GetComponent<BlockController>().SetUsable(true);
                    }
                }
            }
        }
    }

    void AddNewLine()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            // Subir todas las filas hacia arriba
            for (int y = gridHeight - 1; y > 0; y--)
            {
                gridArray[x, y] = gridArray[x, y - 1];
                if (gridArray[x, y] != null)
                {
                    gridArray[x, y].GetComponent<BlockController>().SetGridPosition(new Vector2Int(x, y));
                }
            }

            // Generar una nueva fila en la parte inferior
            GameObject newBlock = Instantiate(GetRandomBlock(x, 0), new Vector3(x, 0, 0), Quaternion.identity);
            newBlock.GetComponent<BlockController>().SetGridPosition(new Vector2Int(x, 0));
            gridArray[x, 0] = newBlock;

            // Marcar el bloque como no usable hasta que suba
            newBlock.GetComponent<BlockController>().SetUsable(false);
            newBlock.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); // Hacerlo opaco
        }
    }


    // Genera la cuadrícula de bloques
    void GenerateGrid()
    {
        gridArray = new GameObject[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < (gridHeight / 2) + 1; y++)
            {
                Vector3 spawnPosition = new Vector3(x, y, 0);
                GameObject block = Instantiate(GetRandomBlock(x, y), spawnPosition, Quaternion.identity);
                block.GetComponent<BlockController>().SetGridPosition(new Vector2Int(x, y));
                gridArray[x, y] = block;
                if (y.Equals(0))
                {
                    block.GetComponent<BlockController>().SetUsable(false);
                }
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

            if (block1 != null && block2 == null) // Si el cubo destino está vacío
            {
                // Mover el cubo origen a la posición destino
                gridArray[block2Pos.x, block2Pos.y] = block1;
                gridArray[block1Pos.x, block1Pos.y] = null;
                block1.GetComponent<BlockController>().SetGridPosition(block2Pos);
                StartCoroutine(SmoothMove(block1, new Vector3(block2Pos.x, block2Pos.y + gridOffset, 0), 0.15f)); // Animar el movimiento
                StartCoroutine(HandleBlockFall()); // Llamar a HandleBlockFall para que los bloques caigan
            }
            else if (block1 != null && block2 != null)
            {
                // Intercambiar los cubos
                gridArray[block1Pos.x, block1Pos.y] = block2;
                gridArray[block2Pos.x, block2Pos.y] = block1;
                block1.GetComponent<BlockController>().SetGridPosition(block2Pos);
                block2.GetComponent<BlockController>().SetGridPosition(block1Pos);
                StartCoroutine(SmoothSwap(block1, block2, block1Pos, block2Pos)); // Animar el intercambio
            }
        }
    }

    bool BlockIsUsable(GameObject block)
    {
        BlockController blockController = block.GetComponent<BlockController>();

        bool canMove = false;

        if (blockController.IsUsable())
        {
            canMove = true;
        }

        return canMove;
    }

    // Corrutina para intercambiar bloques suavemente
    IEnumerator SmoothSwap(GameObject block1, GameObject block2, Vector2Int block1Pos, Vector2Int block2Pos)
    {
        float elapsedTime = 0f;
        float duration = 0.15f;  // Duración de la animación

        Vector3 startPos1 = block1.transform.position;
        Vector3 startPos2 = block2.transform.position;

        // Animación de intercambio suave
        while (elapsedTime < duration)
        {
            if (block1 != null && block2 != null)  // Verificamos que ambos bloques aún existen
            {
                block1.transform.position = Vector3.Lerp(startPos1, startPos2, elapsedTime / duration);
                block2.transform.position = Vector3.Lerp(startPos2, startPos1, elapsedTime / duration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de que ambos bloques lleguen a la posición final
        if (block1 != null) block1.transform.position = startPos2;
        if (block2 != null) block2.transform.position = startPos1;

        // Actualizar las posiciones en la cuadrícula
        if (block1 != null) block1.GetComponent<BlockController>().SetGridPosition(block2Pos);
        if (block2 != null) block2.GetComponent<BlockController>().SetGridPosition(block1Pos);

        // Verificar si hay coincidencias después del intercambio
        StartCoroutine(CheckMatchesAndDestroy(block1Pos, block2Pos));
    }


    // Corrutina para mover bloques suavemente
    IEnumerator SmoothMove(GameObject block, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = block.transform.position;

        while (elapsedTime < duration)
        {
            if (block != null)  // Verificamos que el bloque aún existe
            {
                block.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de que el bloque llegue a la posición final
        if (block != null) block.transform.position = targetPosition;
    }

    // Verifica si hay coincidencias y destruye bloques si las hay
    IEnumerator CheckMatchesAndDestroy(Vector2Int pos1, Vector2Int pos2)
    {
        yield return new WaitForSeconds(0.1f);

        // Verificar si aún existen los bloques antes de intentar operar con ellos
        GameObject block1 = gridArray[pos1.x, pos1.y];
        GameObject block2 = gridArray[pos2.x, pos2.y];

        CheckMatches(pos1);
        CheckMatches(pos2);

        yield return new WaitForSeconds(0.2f);  // Esperamos un poco para que se vea mejor

        // Verificar si aún existen los bloques antes de intentar operar con ellos
        if (block1 != null) block1.GetComponent<BlockController>().SetGridPosition(pos1);
        if (block2 != null) block2.GetComponent<BlockController>().SetGridPosition(pos2);

        StartCoroutine(HandleBlockFall());  // Hacer que los bloques caigan si hay espacio vacío debajo
    }

    // Hacer que los bloques caigan si hay espacio vacío debajo
    IEnumerator HandleBlockFall(List<GameObject> blocksToDestroy = null)
    {
        if (blocksToDestroy != null)
        {
            StartCoroutine(HandleBlockFallAfterDestruction(blocksToDestroy));
        }
        else
        {
            bool hasFallingBlocks = false;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 1; y < gridHeight; y++)  // Comenzamos en y = 1 ya que no tiene nada debajo
                {
                    if (gridArray[x, y] != null)
                    {
                        int dropDistance = 0;

                        // Contamos cuántos espacios vacíos hay debajo
                        while (y - dropDistance - 1 >= 0 && gridArray[x, y - dropDistance - 1] == null)
                        {
                            dropDistance++;
                        }

                        // Si hay un espacio vacío, hacemos que el bloque caiga
                        if (dropDistance > 0)
                        {
                            GameObject block = gridArray[x, y];
                            gridArray[x, y] = null;
                            Vector3 targetPosition = new Vector3(x, y - dropDistance, 0);
                            StartCoroutine(SmoothFall(block, targetPosition, fallSpeed));
                            gridArray[x, y - dropDistance] = block;
                            block.GetComponent<BlockController>().SetGridPosition(new Vector2Int(x, y - dropDistance));

                            hasFallingBlocks = true;  // Indicamos que hubo bloques que cayeron
                        }
                    }
                }
            }

            yield return new WaitForSeconds(fallDelay+0.2f);

            // Si hubo bloques que cayeron, verificamos nuevas coincidencias después de que caigan
            if (hasFallingBlocks)
            {
                StartCoroutine(CheckForNewMatchesAfterFall());
            }
        }
    }

    private IEnumerator HandleBlockFallAfterDestruction(List<GameObject> blocksToDestroy)
    {
        // Esperamos a que todos los bloques se destruyan antes de permitir que caigan los superiores
        yield return new WaitForSeconds(fallDelay /** blocksToDestroy.Count*/);  // Aseguramos que esperamos el tiempo adecuado para la destrucción secuencial

        // Luego llamamos al código que maneja la caída de bloques
        StartCoroutine(HandleBlockFall());
    }



    IEnumerator CheckForNewMatchesAfterFall()
    {
        // Esperamos a que termine el movimiento de caída
        yield return new WaitForSeconds(0.2f);

        bool foundNewMatches = false;

        // Recorremos toda la cuadrícula para verificar coincidencias después de que los bloques cayeron
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
            riseSpeed = 0;

            StartCoroutine(HandleBlockFall());
        }
        else
        {
            riseSpeed = _riseSpeedHolder;
        }
    }



    IEnumerator SmoothFall(GameObject block, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = block.transform.position;

        while (elapsedTime < duration)
        {
            if (block != null)  // Verificamos que el bloque aún existe
            {
                block.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de que el bloque llegue a la posición final
        if (block != null) block.transform.position = targetPosition;
    }


    // Verifica si una posición es válida dentro de la cuadrícula
    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    // Verificar coincidencias (matches) para destruir bloques
    public bool CheckMatches(Vector2Int pos)
    {
        GameObject block = gridArray[pos.x, pos.y];
        if (block == null) return false;

        BlockController blockController = block.GetComponent<BlockController>();

        // Verificar si el bloque es usable
        if (!blockController.IsUsable()) return false;

        SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();
        List<Vector2Int> horizontalMatches = new List<Vector2Int>();
        List<Vector2Int> verticalMatches = new List<Vector2Int>();

        // Buscar coincidencias horizontales
        horizontalMatches.Add(pos);
        for (int i = pos.x - 1; i >= 0; i--)
        {
            if (gridArray[i, pos.y] != null && gridArray[i, pos.y].GetComponent<BlockController>().IsUsable() &&
                gridArray[i, pos.y].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
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
            if (gridArray[i, pos.y] != null && gridArray[i, pos.y].GetComponent<BlockController>().IsUsable() &&
                gridArray[i, pos.y].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
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
            if (gridArray[pos.x, i] != null && gridArray[pos.x, i].GetComponent<BlockController>().IsUsable() &&
                gridArray[pos.x, i].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
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
            if (gridArray[pos.x, i] != null && gridArray[pos.x, i].GetComponent<BlockController>().IsUsable() &&
                gridArray[pos.x, i].GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite)
            {
                verticalMatches.Add(new Vector2Int(pos.x, i));
            }
            else
            {
                break;
            }
        }

        bool matchFound = false;

        // Si hay 3 o más coincidencias horizontales o verticales, marcar los bloques para desaparecer
        List<GameObject> blocksToDestroy = new List<GameObject>();

        if (horizontalMatches.Count >= 3)
        {
            foreach (Vector2Int match in horizontalMatches)
            {
                GameObject matchedBlock = gridArray[match.x, match.y];
                if (matchedBlock != null)
                {
                    blocksToDestroy.Add(matchedBlock);
                    gridArray[match.x, match.y] = null;
                    matchedBlock.GetComponent<BlockController>().SetDissapearing(true);
                }
            }
            matchFound = true;
        }

        if (verticalMatches.Count >= 3)
        {
            foreach (Vector2Int match in verticalMatches)
            {
                GameObject matchedBlock = gridArray[match.x, match.y];
                if (matchedBlock != null && !blocksToDestroy.Contains(matchedBlock))
                {
                    blocksToDestroy.Add(matchedBlock);
                    gridArray[match.x, match.y] = null;
                    matchedBlock.GetComponent<BlockController>().SetDissapearing(true);
                }
            }
            matchFound = true;
        }

        if (matchFound)
        {
            riseSpeed = 0;

            StartCoroutine(DestroyBlocksInSequence(blocksToDestroy));
        }

        return matchFound;
    }

    private IEnumerator DestroyBlocksInSequence(List<GameObject> blocksToDestroy)
    {
        foreach (var block in blocksToDestroy)
        {
            if (block != null)
            {
                // Apagar el sprite antes de destruir
                block.GetComponent<SpriteRenderer>().enabled = false;
                yield return new WaitForSeconds(cubesWaitTime);
            }
        }

        yield return new WaitForSeconds(cubesWaitTime * blocksToDestroy.Count);

        // Esperar un breve periodo antes de destruir los bloques

        // Destruir los bloques después del delay
        foreach (var block in blocksToDestroy)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }

        // Esperar a que todos los bloques hayan sido destruidos antes de permitir que los bloques caigan
        while (blocksToDestroy.Any(block => block != null))
        {
            yield return null;
        }

        // Una vez que todos los bloques han sido destruidos, permitimos que los bloques caigan
        StartCoroutine(HandleBlockFall());
    }
}
