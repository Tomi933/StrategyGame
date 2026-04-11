using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;

    public int width = 14;
    public int height = 11;

    public Transform cellHolder;
    public GameObject cellPrefab;

    public float spacing = 1.1f; // відступ між клітинками

    public float offsetX = -3f; // зміщення вліво
    public float offsetY = -3f; // зміщення вниз

    Cell[,] grid;

    public Cell GetCell(int x, int y) =>
        grid[x, y];

    public void GenerateGrid()
    {
        grid = new Cell[width, height];

        float cellWidth = cellPrefab.transform.localScale.x;
        float cellHeight = cellPrefab.transform.localScale.y;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(
                    x * cellWidth * spacing + offsetX,
                    y * cellHeight * spacing + offsetY
                );

                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, cellHolder);

                Cell cellScript = cell.GetComponent<Cell>();

                if (y < 2)
                {
                    cellScript.isForStartDistribution = true;
                }

                cellScript.x = x;
                cellScript.y = y;

                grid[x, y] = cellScript;
            }
        }
    }

    public List<Cell> GetTopCells()
    {
        List<Cell> cells = new List<Cell>();

        for (int x = 0; x < width; x++)
        {
            for (int y = height - 2; y < height; y++)
            {
                Cell cell = GetCell(x, y);

                if (!cell.isOccupied)
                    cells.Add(cell);
            }
        }

        return cells;
    }


    public void HighlightPlacementCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                var cell = grid[x, y];
                if (cell.isForStartDistribution)
                    cell.SetMoveColor();
            }
        }
    }

    public void UnHighlightPlacementCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                var cell = grid[x, y];
                if (cell.isForStartDistribution)
                    cell.SetBaseColor();
            }
        }
    }
}