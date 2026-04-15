using Assets._Project.Code.Configs.Units;
using System.Collections.Generic;
using UnityEngine;

public enum BotDifficulty { Easy, Medium, Hard }

public class BotAI : MonoBehaviour
{
    private GridManager _gridManager;
    private BotDifficulty _difficulty = BotDifficulty.Medium;

    public void Init(GridManager gridManager, BotDifficulty difficulty = BotDifficulty.Medium)
    {
        _gridManager = gridManager;
        _difficulty = difficulty;
    }

    public void ExecuteTurn()
    {
        switch (_difficulty)
        {
            case BotDifficulty.Easy: ExecuteEasy(); break;
            case BotDifficulty.Medium: ExecuteMedium(); break;
            case BotDifficulty.Hard: ExecuteHard(); break;
        }
    }

    // Easy: якщо може атакувати — атакує рандомного, інакше рандомний юніт іде до гравця
    private void ExecuteEasy()
    {
        var playerUnits = GetPlayerUnits();
        var allBots = GetBotUnits();

        // збираємо всіх ботів які можуть атакувати
        var botsCanAttack = new List<(Unit bot, List<Unit> targets)>();
        foreach (var bot in allBots)
        {
            var targets = bot.GetAttackTargets2(_gridManager, playerUnits);
            if (targets.Count > 0)
                botsCanAttack.Add((bot, targets));
        }

        if (botsCanAttack.Count > 0)
        {
            var (attacker, targets) = botsCanAttack[Random.Range(0, botsCanAttack.Count)];
            attacker.Attack(targets[Random.Range(0, targets.Count)]);
            return;
        }

        // інакше рандомний юніт іде до гравця
        var randomBot = allBots[Random.Range(0, allBots.Count)];
        Unit nearestTarget = FindNearest(randomBot, playerUnits);
        if (nearestTarget == null) return;
        var moveCells = randomBot.FindAvailableCellsForMove(_gridManager);
        Cell bestCell = FindCellClosestTo(moveCells, nearestTarget.currentCell);
        if (bestCell != null)
            randomBot.MoveTo(bestCell);
    }

    // Medium: атакує того в кого найменше хп, якщо кілька цілей — найслабшу, інакше рандомний іде до гравця
    private void ExecuteMedium()
    {
        var playerUnits = GetPlayerUnits();
        var allBots = GetBotUnits();

        var botsCanAttack = new List<(Unit bot, List<Unit> targets)>();
        foreach (var bot in allBots)
        {
            var targets = bot.GetAttackTargets2(_gridManager, playerUnits);
            if (targets.Count > 0)
                botsCanAttack.Add((bot, targets));
        }

        if (botsCanAttack.Count > 0)
        {
            // вибираємо бота у якого противник має найменше хп
            Unit bestAttacker = null;
            Unit weakestOverall = null;
            float minHp = float.MaxValue;

            foreach (var (bot, targets) in botsCanAttack)
            {
                Unit weakest = GetWeakest(targets);
                if (weakest.CurrentHealth < minHp)
                {
                    minHp = weakest.CurrentHealth;
                    bestAttacker = bot;
                    weakestOverall = weakest;
                }
            }

            bestAttacker.Attack(weakestOverall);
            return;
        }

        // інакше рандомний іде до гравця
        var randomBot = allBots[Random.Range(0, allBots.Count)];
        Unit nearestTarget = FindNearest(randomBot, playerUnits);
        if (nearestTarget == null) return;
        var moveCells = randomBot.FindAvailableCellsForMove(_gridManager);
        Cell bestCell = FindCellClosestTo(moveCells, nearestTarget.currentCell);
        if (bestCell != null)
            randomBot.MoveTo(bestCell);
    }

    // Hard: як Medium але з тактичним аналізом — порівнює характеристики і може відступати
    private void ExecuteHard()
    {
        var playerUnits = GetPlayerUnits();
        var allBots = GetBotUnits();

        var botsCanAttack = new List<(Unit bot, List<Unit> targets)>();
        foreach (var bot in allBots)
        {
            var targets = bot.GetAttackTargets2(_gridManager, playerUnits);
            if (targets.Count > 0)
                botsCanAttack.Add((bot, targets));
        }

        if (botsCanAttack.Count > 0)
        {
            Unit bestAttacker = null;
            Unit bestTarget = null;
            float bestScore = float.MinValue;

            foreach (var (bot, targets) in botsCanAttack)
            {
                foreach (var target in targets)
                {
                    float score = EvaluateAttack(bot, target);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestAttacker = bot;
                        bestTarget = target;
                    }
                }
            }

            // якщо атака вигідна — атакуємо
            if (bestScore > 0)
            {
                bestAttacker.Attack(bestTarget);
                return;
            }
        }

        // інакше — тактичний рух: наближаємось або відступаємо
        foreach (var bot in allBots)
        {
            Unit nearestEnemy = FindNearest(bot, playerUnits);
            if (nearestEnemy == null) continue;

            var moveCells = bot.FindAvailableCellsForMove(_gridManager);
            if (moveCells.Count == 0) continue;

            bool shouldRetreat = ShouldRetreat(bot, nearestEnemy);

            Cell targetCell = shouldRetreat
                ? FindCellFarthestFrom(moveCells, nearestEnemy.currentCell)
                : FindCellClosestTo(moveCells, nearestEnemy.currentCell);

            if (targetCell != null)
            {
                bot.MoveTo(targetCell);
                return;
            }
        }
    }

    // рахуємо наскільки вигідно атакувати ціль
    private float EvaluateAttack(Unit bot, Unit target)
    {
        float score = 0;

        // вигідніше атакувати слабкого
        score += (1f / Mathf.Max(1f, target.CurrentHealth)) * 100f;

        // вигідніше якщо наш damage великий відносно хп ворога
        score += bot.Config.damage / Mathf.Max(1f, target.CurrentHealth) * 50f;

        // невигідно атакувати якщо ворог сильніший і може відповісти
        float enemyThreat = target.Config.damage / Mathf.Max(1f, bot.CurrentHealth);
        score -= enemyThreat * 30f;

        // перевага якщо наш range більший
        score += (bot.Config.attackRange - target.Config.attackRange) * 10f;

        return score;
    }

    // чи варто відступати — якщо ворог загрожує і ми слабкі
    private bool ShouldRetreat(Unit bot, Unit nearestEnemy)
    {
        float ourStrength = bot.CurrentHealth * bot.Config.damage;
        float enemyStrength = nearestEnemy.CurrentHealth * nearestEnemy.Config.damage;

        int distToEnemy = Mathf.Abs(nearestEnemy.currentCell.x - bot.currentCell.x)
                        + Mathf.Abs(nearestEnemy.currentCell.y - bot.currentCell.y);

        // відступаємо якщо ворог сильніший і близько
        return enemyStrength > ourStrength * 1.5f && distToEnemy <= nearestEnemy.Config.attackRange + 1;
    }

    private Unit GetWeakest(List<Unit> units)
    {
        Unit weakest = units[0];
        foreach (var u in units)
            if (u.CurrentHealth < weakest.CurrentHealth)
                weakest = u;
        return weakest;
    }

    private Cell FindCellFarthestFrom(List<Cell> cells, Cell target)
    {
        Cell best = null;
        int maxDist = int.MinValue;
        foreach (var c in cells)
        {
            int dist = Mathf.Abs(c.x - target.x) + Mathf.Abs(c.y - target.y);
            if (dist > maxDist) { maxDist = dist; best = c; }
        }
        return best;
    }

    private List<Unit> GetBotUnits()
    {
        var all = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        var bots = new List<Unit>();
        foreach (var u in all)
            if (u.team == Team.Enemy && u.Config.Behavior != UnitBehavior.Static)
                bots.Add(u);
        return bots;
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