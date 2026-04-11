using Assets._Project.Code.Configs.Units;
using Assets._Project.Code.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public enum UnitMoveType { Cross, Circle }
public enum UnitAttackType { Cross, Circle }

public class Unit : MonoBehaviour
{
    [SerializeField] private HealthBarUI _healthBar;

    public Team team;
    public UnitConfigSO Config => _unitConfig;

    [HideInInspector] public Cell currentCell;

    public int scanRange = 3;

    private UnitConfigSO _unitConfig;


    public void Init(Cell cell, UnitConfigSO unitConfig)
    {
        currentCell = cell;
        cell.unit = this;
        _unitConfig = unitConfig;

        _healthBar.Init(Mathf.Max(1f, _unitConfig.Health));
        _healthBar.OnDied += OnDied;
    }

    private void OnDied()
    {
        currentCell.isOccupied = false;
        currentCell.unit = null;
        Destroy(gameObject);
    }

    public void TakeDamage(float dmg) => 
        _healthBar.Reduce(dmg);

    public List<Cell> FindAvailableCellsForMove(GridManager grid)
    {
        List<Cell> result = new List<Cell>();
        var moveRange = _unitConfig.MoveRange;

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                int checkX;
                int checkY;

                if (_unitConfig.MoveType == UnitMoveType.Cross)
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


    public List<Cell> GetAttackCells(GridManager grid, Team team)
    {
        List<Cell> result = new List<Cell>();

        for (int x = -Config.attackRange; x <= Config.attackRange; x++)
        {
            for (int y = -Config.attackRange; y <= Config.attackRange; y++)
            {
                int checkX;
                int checkY;

                if (Config.AttackType == UnitAttackType.Cross)
                {
                    if (x == 0 && y == 0) continue;

                    if (x != 0 && y != 0) continue;

                    checkX = currentCell.x + x;
                    checkY = currentCell.y + y;

                    if (checkX >= 0 && checkX < grid.width &&
                        checkY >= 0 && checkY < grid.height)
                    {
                        Cell cell = grid.GetCell(checkX, checkY);

                        if (cell == currentCell) continue;

                        if (cell.unit != null && cell.unit.team == team) continue;

                        result.Add(cell);
                    }
                }
                else
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) > Config.attackRange)
                        continue;

                    checkX = currentCell.x + x;
                    checkY = currentCell.y + y;

                    if (checkX >= 0 && checkX < grid.width &&
                        checkY >= 0 && checkY < grid.height)
                    {
                        Cell cell = grid.GetCell(checkX, checkY);

                        if (cell == currentCell) continue;

                        if (cell.unit != null && cell.unit.team == team) continue;

                        result.Add(cell);
                    }
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

    public List<Unit> GetAttackTargets(GridManager grid, Team team)
    {
        List<Unit> targets = new List<Unit>();

        var cells = GetAttackCells(grid, team);

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
        target.TakeDamage(Config.damage);
    }


    public List<Unit> GetAttackTargets2(GridManager grid, List<Unit> allUnits)
    {
        var targets = new List<Unit>();
        foreach (var unit in allUnits)
        {
            if (unit.team == this.team) continue; // атакуємо тільки ворогів
            int distance = Mathf.Abs(unit.currentCell.x - currentCell.x) +
                           Mathf.Abs(unit.currentCell.y - currentCell.y);
            if (distance <= Config.attackRange)
                targets.Add(unit);
        }
        return targets;
    }

    private void OnDestroy()
    {
        _healthBar.OnDied -= OnDied;
    }
}