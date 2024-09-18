using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public enum BlockType { Red, Blue, Green, Yellow, Purple }; // Diferentes tipos de bloques
    public BlockType blockType; // Tipo de bloque

    private Vector2Int gridPosition;

    private bool isUsable = true; // Los bloques por defecto ser�n usables al inicio
    void Start()
    {
        // Aseg�rate de tener una referencia al SpriteRenderer para cambiar su color cuando sea necesario
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, isUsable ? 1f : 0.5f);
    }


    // M�todo para marcar si el bloque es usable o no
    public void SetUsable(bool usable)
    {
        isUsable = usable;
    }

    // M�todo para verificar si el bloque es usable
    public bool IsUsable()
    {
        return isUsable;
    }

    // Establecer la posici�n del bloque en la grilla
    public void SetGridPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
    }

    // Obtener la posici�n del bloque en la grilla
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
}
