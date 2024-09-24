using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public enum BlockType { Pink, Blue, Green, Orange, Purple, White }; // Diferentes tipos de bloques
    public BlockType blockType; // Tipo de bloque
    public BoxCollider2D[] boxColliders;

    private Vector2Int gridPosition;
    private bool isDissapearing = false;

    private bool isUsable = true; // Los bloques por defecto serán usables al inicio
    private SpriteRenderer spriteRenderer;

    public bool IsBusy = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        
        UpdateColor(); // Actualiza el color al inicio basado en usabilidad
    }

    // Método para actualizar el color del bloque según su estado de usabilidad
    private void UpdateColor()
    {
        if (isUsable)
        {
            spriteRenderer.color = new Color(1, 1, 1, 1f); // Color normal si es usable
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f); // Color semitransparente si no es usable
        }
    }

    // Método para marcar si el bloque es usable o no
    public void SetUsable(bool usable)
    {
        isUsable = usable;
        UpdateColor(); // Actualiza el color cuando cambie la usabilidad

        if (isUsable)
        {
            // Referencia a GridManager para verificar coincidencias
            GameController.Instance.gridManager.CheckMatches(gridPosition);
        }
    }

    // Método para verificar si el bloque es usable
    public bool IsUsable()
    {
        return isUsable;
    }

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

    public void SetDissapearing(bool dissapearing)
    {
        isDissapearing = dissapearing;
        if (isDissapearing)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.25f); // Color semitransparente cuando desaparece
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1, 1f); // Color normal cuando no desaparece
        }
    }

    public bool IsDissapearing()
    {
        return isDissapearing;
    }

    public void SetBusy(bool status)
    {
        IsBusy = status;
    }
}
