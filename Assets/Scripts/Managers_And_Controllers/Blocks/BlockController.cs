using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public enum BlockType { Red, Blue, Green, Yellow, Purple }; // Diferentes tipos de bloques
    public BlockType blockType; // Tipo de bloque

    private Vector2Int gridPosition;

    // Establecer la posición del bloque en la grilla
    public void SetGridPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
    }

    // Obtener la posición del bloque en la grilla
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
}
