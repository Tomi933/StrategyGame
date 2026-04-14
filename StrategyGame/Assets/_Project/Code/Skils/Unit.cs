using Assets._Project.Code.Configs.Units;
using Assets._Project.Code.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public enum UnitMoveType { Cross, Stick, Star }
public enum UnitAttackType { Splash, Star, SubCross }
public enum UnitScanType { Star, Circle }

public class Unit : MonoBehaviour
{
    [SerializeField] private HealthBarUI _healthBar;
    [SerializeField] private GameObject _model;

    public Team team;
    public UnitConfigSO Config => _unitConfig;

    [HideInInspector] public Cell currentCell;

    private UnitConfigSO _unitConfig;

    public event Action OnDiedEvent;

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
        OnDiedEvent?.Invoke();
        Destroy(gameObject);
    }

    public void TakeDamage(float dmg) => 
        _healthBar.Reduce(dmg);

 
    public List<Cell> FindAvailableCellsForMove(GridManager grid)
    {
        List<Cell> result = new List<Cell>();
        var moveRange = _unitConfig.MoveRange;

        switch (_unitConfig.MoveType)
        {
            case UnitMoveType.Cross:
                // Тільки по горизонталі або вертикалі, залежно від радіусу
                for (int i = -moveRange; i <= moveRange; i++)
                {
                    if (i == 0) continue;
                    TryAddCell(grid, result, currentCell.x + i, currentCell.y); // горизонталь
                    TryAddCell(grid, result, currentCell.x, currentCell.y + i); // вертикаль
                }
                break;

            case UnitMoveType.Stick:
                // Зверху 2, знизу 2 — фіксовано
                TryAddCell(grid, result, currentCell.x, currentCell.y + 1);
                TryAddCell(grid, result, currentCell.x, currentCell.y + 2);
                TryAddCell(grid, result, currentCell.x, currentCell.y - 1);
                // По боках по 1 — фіксовано
                TryAddCell(grid, result, currentCell.x - 1, currentCell.y);
                TryAddCell(grid, result, currentCell.x + 1, currentCell.y);
                break;

            case UnitMoveType.Star:
                // По горизонталі та вертикалі залежно від радіусу
                for (int i = -moveRange; i <= moveRange; i++)
                {
                    if (i == 0) continue;
                    TryAddCell(grid, result, currentCell.x + i, currentCell.y); // горизонталь
                    TryAddCell(grid, result, currentCell.x, currentCell.y + i); // вертикаль
                }
                // По діагоналях залежно від радіусу
                for (int i = 1; i <= moveRange; i++)
                {
                    TryAddCell(grid, result, currentCell.x + i, currentCell.y + i); // ↗
                    TryAddCell(grid, result, currentCell.x + i, currentCell.y - i); // ↘
                    TryAddCell(grid, result, currentCell.x - i, currentCell.y + i); // ↖
                    TryAddCell(grid, result, currentCell.x - i, currentCell.y - i); // ↙
                }
                break;
        }

        return result;
    }

    private void TryAddCell(GridManager grid, List<Cell> result, int x, int y)
    {
        if (x >= 0 && x < grid.width && y >= 0 && y < grid.height)
        {
            Cell cell = grid.GetCell(x, y);
            if (!cell.isOccupied)
                result.Add(cell);
        }
    }

    public List<Cell> GetAttackCells(GridManager grid, Team team)
    {
        List<Cell> result = new List<Cell>();
        var range = Config.attackRange;

        switch (Config.AttackType)
        {
            case UnitAttackType.Splash:
                // По горизонталі та вертикалі на 1 клітинку навколо
                for (int i = -1; i <= 1; i++)
                {
                    if (i == 0) continue;
                    TryAddAttackCell(grid, team, result, currentCell.x + i, currentCell.y); // горизонталь ±1
                    TryAddAttackCell(grid, team, result, currentCell.x, currentCell.y + i); // вертикаль ±1
                }
                // По горизонталі та вертикалі на 2 клітинки
                TryAddAttackCell(grid, team, result, currentCell.x + 2, currentCell.y); // право 2
                TryAddAttackCell(grid, team, result, currentCell.x - 2, currentCell.y); // ліво 2
                TryAddAttackCell(grid, team, result, currentCell.x, currentCell.y + 2); // верх 2
                TryAddAttackCell(grid, team, result, currentCell.x, currentCell.y - 2); // низ 2
                break;

            case UnitAttackType.SubCross:
                // Хрест залежно від радіусу по горизонталі та вертикалі
                for (int i = -range; i <= range; i++)
                {
                    if (i == 0) continue;
                    TryAddAttackCell(grid, team, result, currentCell.x + i, currentCell.y); // горизонталь
                    TryAddAttackCell(grid, team, result, currentCell.x, currentCell.y + i); // вертикаль
                }
                // Кути: {±range, ±range}
                TryAddAttackCell(grid, team, result, currentCell.x + range, currentCell.y + range); // ↗
                TryAddAttackCell(grid, team, result, currentCell.x - range, currentCell.y + range); // ↖
                TryAddAttackCell(grid, team, result, currentCell.x + range, currentCell.y - range); // ↘
                TryAddAttackCell(grid, team, result, currentCell.x - range, currentCell.y - range); // ↙
                break;

            case UnitAttackType.Star:
                // По горизонталі та вертикалі залежно від радіусу
                for (int i = -range; i <= range; i++)
                {
                    if (i == 0) continue;
                    TryAddAttackCell(grid, team, result, currentCell.x + i, currentCell.y); // горизонталь
                    TryAddAttackCell(grid, team, result, currentCell.x, currentCell.y + i); // вертикаль
                }
                // По діагоналях залежно від радіусу
                for (int i = 1; i <= range; i++)
                {
                    TryAddAttackCell(grid, team, result, currentCell.x + i, currentCell.y + i); // ↗
                    TryAddAttackCell(grid, team, result, currentCell.x + i, currentCell.y - i); // ↘
                    TryAddAttackCell(grid, team, result, currentCell.x - i, currentCell.y + i); // ↖
                    TryAddAttackCell(grid, team, result, currentCell.x - i, currentCell.y - i); // ↙
                }
                break;
        }

        return result;
    }

    private void TryAddAttackCell(GridManager grid, Team team, List<Cell> result, int x, int y)
    {
        if (x < 0 || x >= grid.width || y < 0 || y >= grid.height) return;
        Cell cell = grid.GetCell(x, y);
        if (cell == currentCell) return;
        if (cell.unit != null && cell.unit.team == team) return;
        result.Add(cell);
    }

    public List<Cell> GetScanCells(GridManager grid)
    {
        List<Cell> result = new List<Cell>();
        var scanRange = Mathf.Max(1, Config.scanRange);

        switch (_unitConfig.ScanType)
        {
            case UnitScanType.Star:
                // По горизонталі та вертикалі + діагоналі залежно від радіусу
                for (int i = -scanRange; i <= scanRange; i++)
                {
                    if (i == 0) continue;
                    TryAddScanCell(grid, result, currentCell.x + i, currentCell.y); // горизонталь
                    TryAddScanCell(grid, result, currentCell.x, currentCell.y + i); // вертикаль
                }
                for (int i = 1; i <= scanRange; i++)
                {
                    TryAddScanCell(grid, result, currentCell.x + i, currentCell.y + i); // ↗
                    TryAddScanCell(grid, result, currentCell.x + i, currentCell.y - i); // ↘
                    TryAddScanCell(grid, result, currentCell.x - i, currentCell.y + i); // ↖
                    TryAddScanCell(grid, result, currentCell.x - i, currentCell.y - i); // ↙
                }
                break;

            case UnitScanType.Circle:
                // Квадратна рамка: всі клітинки де Max(|x|,|y|) <= scanRange
                for (int x = -scanRange; x <= scanRange; x++)
                {
                    for (int y = -scanRange; y <= scanRange; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        TryAddScanCell(grid, result, currentCell.x + x, currentCell.y + y);
                    }
                }
                break;
        }

        return result;
    }

    private void TryAddScanCell(GridManager grid, List<Cell> result, int x, int y)
    {
        if (x < 0 || x >= grid.width || y < 0 || y >= grid.height) return;
        result.Add(grid.GetCell(x, y));
    }


    //public List<Unit> GetAttackTargets(GridManager grid, Team team)
    //{
    //    List<Unit> targets = new List<Unit>();

    //    var cells = GetAttackCells(grid, team);

    //    foreach (var cell in cells)
    //    {
    //        if (cell.isOccupied)
    //        {
    //            Unit unit = cell.GetComponentInChildren<Unit>();

    //            if (unit != null && unit != this && unit.team != this.team)
    //            {
    //                targets.Add(unit);
    //            }
    //        }
    //    }

    //    return targets;
    //}

    public void MoveTo(Cell targetCell)
    {
        currentCell.isOccupied = false;
        currentCell.unit = null;

        transform.position = targetCell.transform.position;
        transform.SetParent(targetCell.transform);

        targetCell.isOccupied = true;
        targetCell.unit = this;
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

    public void SetModelVisible(bool visible)
    {
        _model.SetActive(visible);
    }

    private void OnDestroy()
    {
        _healthBar.OnDied -= OnDied;
    }
}