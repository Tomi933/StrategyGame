using UnityEngine;
using System.Collections.Generic;
using System;

public enum UnitMoveType { Cross, Circle }

public class Unit : MonoBehaviour
{
    public Team team;

    public int maxHP = 10;
    public int currentHP;

    public int damage = 3;
    public int attackRange = 1;

    public int moveRange = 2; 
    public Cell currentCell;

    public int scanRange = 3;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void Init(Cell cell)
    {
        currentCell = cell;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (currentCell == null)
        {
            Debug.LogError("Unit помирає без currentCell!");
        }
        else
        {
            currentCell.isOccupied = false;
        }

        Destroy(gameObject);
    }

    public List<Cell> GetAvailableCellsFor(GridManager grid, UnitMoveType type)
    {
        List<Cell> result = new List<Cell>();

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                int checkX;
                int checkY;

                if (type == UnitMoveType.Cross)
                {
                    if (x == 0 && y == 0) continue;

                    if (x != 0 && y != 0) continue;

                    checkX = currentCell.x + x;
                    checkY = currentCell.y + y;

                    if (checkX >= 0 && checkX < grid.width &&
                        checkY >= 0 && checkY < grid.height)
                    {
                        Cell cell = grid.GetCell(checkX, checkY);

                        if (!cell.isOccupied)
                            result.Add(cell);
                    }
                }
                else
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) > moveRange)
                        continue;

                     checkX = currentCell.x + x;
                     checkY = currentCell.y + y;

                    if (checkX >= 0 && checkX < grid.width &&
                        checkY >= 0 && checkY < grid.height)
                    {
                        Cell cell = grid.GetCell(checkX, checkY);

                        if (!cell.isOccupied)
                            result.Add(cell);
                    }
                }
            }
        }

        return result;
    }


    public List<Cell> GetAttackCells(GridManager grid)
    {
        List<Cell> result = new List<Cell>();

        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > attackRange)
                    continue;

                int checkX = currentCell.x + x;
                int checkY = currentCell.y + y;

                if (checkX >= 0 && checkX < grid.width &&
                    checkY >= 0 && checkY < grid.height)
                {
                    Cell cell = grid.GetCell(checkX, checkY);

                    result.Add(cell);
                }
            }
        }

        return result;
    }

    public List<Cell> GetScanCells(GridManager grid)
    {
        List<Cell> result = new List<Cell>();

        for (int x = -scanRange; x <= scanRange; x++)
        {
            for (int y = -scanRange; y <= scanRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > scanRange)
                    continue;

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

    public List<Unit> GetAttackTargets(GridManager grid)
    {
        List<Unit> targets = new List<Unit>();

        var cells = GetAttackCells(grid);

        foreach (var cell in cells)
        {
            if (cell.isOccupied)
            {
                Unit unit = cell.GetComponentInChildren<Unit>();

                if (unit != null && unit != this && unit.team != this.team)
                {
                    targets.Add(unit);
                }
            }
        }

        return targets;
    }

    public void MoveTo(Cell targetCell)
    {
        currentCell.isOccupied = false;

        transform.position = targetCell.transform.position;
        transform.SetParent(targetCell.transform);

        targetCell.isOccupied = true;
        currentCell = targetCell;
    }

    public void Attack(Unit target)
    {
        target.TakeDamage(damage);
    }


    public List<Unit> GetAttackTargets2(GridManager grid, List<Unit> allUnits)
    {
        var targets = new List<Unit>();
        foreach (var unit in allUnits)
        {
            if (unit.team == this.team) continue; // атакуємо тільки ворогів
            int distance = Mathf.Abs(unit.currentCell.x - currentCell.x) +
                           Mathf.Abs(unit.currentCell.y - currentCell.y);
            if (distance <= attackRange)
                targets.Add(unit);
        }
        return targets;
    }

}