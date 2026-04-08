using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public int moveRange = 2; 
    public Cell currentCell;

    public void Init(Cell cell)
    {
        currentCell = cell;
    }

    public List<Cell> GetAvailableCells(GridManager grid)
    {
        List<Cell> result = new List<Cell>();

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                int checkX = currentCell.x + x;
                int checkY = currentCell.y + y;

                if (checkX >= 0 && checkX < grid.width &&
                    checkY >= 0 && checkY < grid.height)
                {
                    Cell cell = grid.GetCell(checkX, checkY);

                    if (!cell.isOccupied)
                        result.Add(cell);
                }
            }
        }

        return result;
    }

    public void MoveTo(Cell targetCell)
    {
        currentCell.isOccupied = false;

        transform.position = targetCell.transform.position;
        transform.SetParent(targetCell.transform);

        targetCell.isOccupied = true;
        currentCell = targetCell;
    }
}