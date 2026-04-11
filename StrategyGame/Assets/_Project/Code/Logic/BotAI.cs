using Assets._Project.Code.Configs.Units;
using System.Collections.Generic;
using UnityEngine;

public class BotAI : MonoBehaviour
{
    private GridManager _gridManager;

    public void Init(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    public void ExecuteTurn()
    {
        var bot = GetRandomBotUnit();
        var playerUnits = GetPlayerUnits();

        var targets = bot.GetAttackTargets2(_gridManager, playerUnits);

        if (targets.Count > 0)
        {
            bot.Attack(targets[0]);
        }
        else
        {
            Unit nearest = FindNearest(bot, playerUnits);
            if (nearest == null) return;

            var moveCells = bot.FindAvailableCellsForMove(_gridManager);
            Cell bestCell = FindCellClosestTo(moveCells, nearest.currentCell);
            if (bestCell != null)
                bot.MoveTo(bestCell);
        }
    }

    private Unit GetRandomBotUnit()
    {
        var all = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        var bots = new List<Unit>();

        foreach (var u in all)
        {
            if (u.team == Team.Enemy && u.Config.Behavior != UnitBehavior.Static) 
                bots.Add(u);
        }

        var randomIndex = Random.Range(0, bots.Count);
        var randomUnit = bots[randomIndex];

        return randomUnit;
    }

    private List<Unit> GetPlayerUnits()
    {
        var all = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        var players = new List<Unit>();
        foreach (var u in all)
            if (u.team == Team.Player) players.Add(u);
        return players;
    }

    private Unit FindNearest(Unit bot, List<Unit> targets)
    {
        Unit nearest = null;
        int minDist = int.MaxValue;
        foreach (var t in targets)
        {
            int dist = Mathf.Abs(t.currentCell.x - bot.currentCell.x)
                     + Mathf.Abs(t.currentCell.y - bot.currentCell.y);
            if (dist < minDist) { minDist = dist; nearest = t; }
        }
        return nearest;
    }

    private Cell FindCellClosestTo(List<Cell> cells, Cell target)
    {
        Cell best = null;
        int minDist = int.MaxValue;
        foreach (var c in cells)
        {
            int dist = Mathf.Abs(c.x - target.x) + Mathf.Abs(c.y - target.y);
            if (dist < minDist) { minDist = dist; best = c; }
        }
        return best;
    }
}