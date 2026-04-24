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

    public void ApplyDifficultyMultipliers()
    {
        if (_difficulty != BotDifficulty.Hard) return;

        var bots = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var u in bots)
            if (u.team == Team.Enemy)
                u.ApplyHardMultipliers();
    }

    public void ExecuteTurn()
    {
        var bots = GetBotUnits();
        if (bots.Count == 0) return;

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
        ShuffleList(allBots);

        Unit bestAttacker = null;
        Unit bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var bot in allBots)
        {
            var targets = bot.GetAttackTargets2(_gridManager, playerUnits);
            if (targets.Count == 0) continue;

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

        if (bestAttacker != null)
        {
            // знаходимо всіх ворогів які можуть атакувати bestAttacker
            var threats = GetThreatsTo(bestAttacker, playerUnits);

            if (threats.Count > 0)
            {
                // рахуємо небезпеку
                float totalThreatDamage = 0;
                foreach (var t in threats)
                    totalThreatDamage += t.Config.damage;

                bool willDieNextTurn = totalThreatDamage >= bestAttacker.CurrentHealth;

                // шукаємо безпечну клітинку
                Cell safeCell = FindSafeCell(bestAttacker, playerUnits);

                bool canKillTarget = bestTarget.CurrentHealth <= bestAttacker.Config.damage;
                if (safeCell != null && willDieNextTurn && !canKillTarget)
                {
                    // відступаємо
                    bestAttacker.MoveTo(safeCell);
                    return;
                }

                // інакше атакуємо найнебезпечнішого
                Unit mostDangerous = GetMostDangerous(threats, bestAttacker);
                var targetsOfAttacker = bestAttacker.GetAttackTargets2(_gridManager, playerUnits);

                if (targetsOfAttacker.Contains(mostDangerous))
                {
                    bestAttacker.Attack(mostDangerous);
                    return;
                }
            }

            bestAttacker.Attack(bestTarget);
            return;
        }

        // рух якщо ніхто не атакував
        foreach (var bot in allBots)
        {
            Unit nearestEnemy = FindNearest(bot, playerUnits);
            if (nearestEnemy == null) continue;

            var moveCells = bot.FindAvailableCellsForMove(_gridManager);
            if (moveCells.Count == 0) continue;

            bool shouldRetreat = ShouldRetreat(bot, nearestEnemy);

            Cell targetCell = shouldRetreat
                ? FindSafeRetreatCell(moveCells, nearestEnemy.currentCell, allBots, bot)
                : FindCellInAttackRange(moveCells, nearestEnemy, bot)
                  ?? FindCellClosestTo(moveCells, nearestEnemy.currentCell);

            if (targetCell != null)
            {
                bot.MoveTo(targetCell);
                return;
            }
        }
    }

    private void ExecuteHard()
    {
        var playerUnits = GetPlayerUnits();
        var allBots = GetBotUnits();
        ShuffleList(allBots);

        Unit bestAttacker = null;
        Unit bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var bot in allBots)
        {
            var targets = bot.GetAttackTargets2(_gridManager, playerUnits);
            if (targets.Count == 0) continue;

            foreach (var target in targets)
            {
                float score = EvaluateAttack(bot, target);

                // штраф якщо після атаки бот під загрозою
                var threats = GetThreatsTo(bot, playerUnits);
                float totalThreatDamage = 0;
                foreach (var t in threats) totalThreatDamage += t.Config.damage;
                if (totalThreatDamage >= bot.CurrentHealth)
                    score -= 50f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAttacker = bot;
                    bestTarget = target;
                }
            }
        }

        if (bestAttacker != null)
        {
            var threats = GetThreatsTo(bestAttacker, playerUnits);

            if (threats.Count > 0)
            {
                float totalThreatDamage = 0;
                foreach (var t in threats) totalThreatDamage += t.Config.damage;
                bool willDieNextTurn = totalThreatDamage >= bestAttacker.CurrentHealth;

                Cell safeCell = FindSafeCellHard(bestAttacker, playerUnits);

                bool canKillTarget = bestTarget.CurrentHealth <= bestAttacker.Config.damage;

                if (safeCell != null && willDieNextTurn && !canKillTarget)
                {
                    bestAttacker.MoveTo(safeCell);
                    return;
                }

                Unit mostDangerous = GetMostDangerous(threats, bestAttacker);
                var targetsOfAttacker = bestAttacker.GetAttackTargets2(_gridManager, playerUnits);

                if (targetsOfAttacker.Contains(mostDangerous))
                {
                    bestAttacker.Attack(mostDangerous);
                    return;
                }
            }

            bestAttacker.Attack(bestTarget);
            return;
        }

        // рух — шукаємо клітинку з якої можемо атакувати але нас не можуть атакувати
        foreach (var bot in allBots)
        {
            Unit nearestEnemy = FindNearest(bot, playerUnits);
            if (nearestEnemy == null) continue;

            var moveCells = bot.FindAvailableCellsForMove(_gridManager);
            if (moveCells.Count == 0) continue;

            // збираємо небезпечні клітинки
            var dangerCells = new HashSet<Cell>();
            foreach (var enemy in playerUnits)
            {
                var attackCells = enemy.GetAttackCells(_gridManager, enemy.team);
                foreach (var cell in attackCells)
                    dangerCells.Add(cell);
            }

            // шукаємо найкращу клітинку: безпечна + близько до атаки
            Cell bestCell = null;
            float bestCellScore = float.MinValue;

            foreach (var cell in moveCells)
            {
                float cellScore = 0;

                // великий бонус якщо клітинка не під атакою
                bool isSafe = !dangerCells.Contains(cell);
                cellScore += isSafe ? 100f : -50f;

                // бонус за близькість до ворога в межах нашого attackRange
                int distToEnemy = Mathf.Abs(cell.x - nearestEnemy.currentCell.x)
                                + Mathf.Abs(cell.y - nearestEnemy.currentCell.y);

                // ідеально стати рівно на attackRange від ворога
                int distFromIdeal = Mathf.Abs(distToEnemy - bot.Config.attackRange);
                cellScore -= distFromIdeal * 5f;

                // перевіряємо чи ворог потрапляє в зону атаки з нової клітинки
                bool canAttackFromCell = CanAttackFromCell(cell, bot, nearestEnemy);
                cellScore += canAttackFromCell ? 80f : 0f;

                if (cellScore > bestCellScore)
                {
                    bestCellScore = cellScore;
                    bestCell = cell;
                }
            }

            // якщо найкраща клітинка небезпечна — краще стояти на місці
            if (bestCell != null && dangerCells.Contains(bestCell) && !dangerCells.Contains(bot.currentCell))
                continue;

            if (bestCell != null)
            {
                bot.MoveTo(bestCell);
                return;
            }
        }
    }

    // перевіряємо чи зможемо атакувати ворога стоячи на cell
    private bool CanAttackFromCell(Cell fromCell, Unit bot, Unit enemy)
    {
        int dist = Mathf.Abs(fromCell.x - enemy.currentCell.x)
                 + Mathf.Abs(fromCell.y - enemy.currentCell.y);

        switch (bot.Config.AttackType)
        {
            case UnitAttackType.Star:
                bool onAxis = fromCell.x == enemy.currentCell.x || fromCell.y == enemy.currentCell.y;
                bool onDiag = Mathf.Abs(fromCell.x - enemy.currentCell.x) == Mathf.Abs(fromCell.y - enemy.currentCell.y);
                return (onAxis || onDiag) && dist <= bot.Config.attackRange;

            case UnitAttackType.SubCross:
                bool axis = fromCell.x == enemy.currentCell.x || fromCell.y == enemy.currentCell.y;
                bool corner = Mathf.Abs(fromCell.x - enemy.currentCell.x) == bot.Config.attackRange
                           && Mathf.Abs(fromCell.y - enemy.currentCell.y) == bot.Config.attackRange;
                return (axis && dist <= bot.Config.attackRange) || corner;

            case UnitAttackType.Splash:
                return dist <= 2 && (fromCell.x == enemy.currentCell.x || fromCell.y == enemy.currentCell.y);

            default:
                return dist <= bot.Config.attackRange;
        }
    }

    // Hard версія FindSafeCell — шукає клітинку з якої можна атакувати і яка не під атакою
    private Cell FindSafeCellHard(Unit bot, List<Unit> enemies)
    {
        var moveCells = bot.FindAvailableCellsForMove(_gridManager);
        if (moveCells.Count == 0) return null;

        var dangerCells = new HashSet<Cell>();
        foreach (var enemy in enemies)
        {
            var attackCells = enemy.GetAttackCells(_gridManager, enemy.team);
            foreach (var cell in attackCells)
                dangerCells.Add(cell);
        }

        Cell best = null;
        float bestScore = float.MinValue;

        foreach (var cell in moveCells)
        {
            if (dangerCells.Contains(cell)) continue;

            float score = 0;

            // бонус якщо з цієї клітинки можемо атакувати когось
            foreach (var enemy in enemies)
            {
                if (CanAttackFromCell(cell, bot, enemy))
                {
                    score += 100f;
                    // більший бонус за слабкого ворога
                    score += (1f / Mathf.Max(1f, enemy.CurrentHealth)) * 50f;
                }
            }

            // бонус за відстань від найближчого ворога
            foreach (var enemy in enemies)
            {
                int d = Mathf.Abs(cell.x - enemy.currentCell.x)
                      + Mathf.Abs(cell.y - enemy.currentCell.y);
                score += d * 2f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = cell;
            }
        }

        return best;
    }
    // всі вороги які можуть атакувати цього юніта
    private List<Unit> GetThreatsTo(Unit bot, List<Unit> enemies)
    {
        var threats = new List<Unit>();
        foreach (var enemy in enemies)
        {
            var enemyTargets = enemy.GetAttackTargets2(_gridManager, new List<Unit> { bot });
            if (enemyTargets.Count > 0)
                threats.Add(enemy);
        }
        return threats;
    }

    // найнебезпечніший ворог — той хто за найменше ходів знищить бота
    private Unit GetMostDangerous(List<Unit> threats, Unit bot)
    {
        Unit mostDangerous = null;
        float maxDanger = float.MinValue;

        foreach (var threat in threats)
        {
            // скільки ходів потрібно щоб знищити бота
            float turnsToKillBot = bot.CurrentHealth / Mathf.Max(1f, threat.Config.damage);
            // скільки ходів потрібно боту щоб знищити загрозу
            float turnsToKillThreat = threat.CurrentHealth / Mathf.Max(1f, bot.Config.damage);

            // небезпечніший той хто швидше вб'є і важче вбити
            float danger = (1f / turnsToKillBot) * 50f + turnsToKillThreat * 10f;

            if (danger > maxDanger)
            {
                maxDanger = danger;
                mostDangerous = threat;
            }
        }

        return mostDangerous;
    }

    // клітинка яку жоден ворог не б'є
    private Cell FindSafeCell(Unit bot, List<Unit> enemies)
    {
        var moveCells = bot.FindAvailableCellsForMove(_gridManager);
        if (moveCells.Count == 0) return null;

        // збираємо всі клітинки які покривають вороги
        var dangerCells = new HashSet<Cell>();
        foreach (var enemy in enemies)
        {
            var attackCells = enemy.GetAttackCells(_gridManager, enemy.team);
            foreach (var cell in attackCells)
                dangerCells.Add(cell);
        }

        // шукаємо безпечну клітинку якнайдалі від ворогів
        Cell best = null;
        int maxDist = int.MinValue;

        foreach (var cell in moveCells)
        {
            if (dangerCells.Contains(cell)) continue;

            int distFromNearestEnemy = int.MaxValue;
            foreach (var enemy in enemies)
            {
                int d = Mathf.Abs(cell.x - enemy.currentCell.x)
                      + Mathf.Abs(cell.y - enemy.currentCell.y);
                if (d < distFromNearestEnemy)
                    distFromNearestEnemy = d;
            }

            if (distFromNearestEnemy > maxDist)
            {
                maxDist = distFromNearestEnemy;
                best = cell;
            }
        }

        return best;
    }

    


    // шукаємо клітинку з якої зможемо атакувати ворога наступного ходу
    private Cell FindCellInAttackRange(List<Cell> moveCells, Unit enemy, Unit bot)
    {
        Cell best = null;
        int bestDist = int.MaxValue;

        foreach (var cell in moveCells)
        {
            int dist = Mathf.Abs(cell.x - enemy.currentCell.x)
                     + Mathf.Abs(cell.y - enemy.currentCell.y);

            // хочемо стати на відстані attackRange від ворога
            int idealDist = Mathf.Abs(dist - bot.Config.attackRange);
            if (idealDist < bestDist)
            {
                bestDist = idealDist;
                best = cell;
            }
        }

        return best;
    }

    // відступаємо але залишаємось близько до союзників
    private Cell FindSafeRetreatCell(List<Cell> moveCells, Cell enemyCell, List<Unit> allBots, Unit self)
    {
        Cell best = null;
        float bestScore = float.MinValue;

        foreach (var cell in moveCells)
        {
            int distFromEnemy = Mathf.Abs(cell.x - enemyCell.x)
                              + Mathf.Abs(cell.y - enemyCell.y);

            // бонус за відстань від ворога
            float score = distFromEnemy * 2f;

            // штраф за відрив від союзників
            foreach (var ally in allBots)
            {
                if (ally == self) continue;
                int distFromAlly = Mathf.Abs(cell.x - ally.currentCell.x)
                                 + Mathf.Abs(cell.y - ally.currentCell.y);
                score -= distFromAlly * 0.5f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = cell;
            }
        }

        return best ?? FindCellFarthestFrom(moveCells, enemyCell);
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Hard: як Medium але з тактичним аналізом — порівнює характеристики і може відступати
    

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

        // відступаємо тільки якщо:
        // 1. ворог ЗНАЧНО сильніший (в 2 рази, а не 1.5)
        // 2. ворог в зоні атаки прямо зараз
        // 3. ми не можемо його атакувати першими
        bool enemyMuchStronger = enemyStrength > ourStrength * 2f;
        bool enemyCanAttackNow = distToEnemy <= nearestEnemy.Config.attackRange;
        bool weCanAttackFirst = distToEnemy <= bot.Config.attackRange;

        return enemyMuchStronger && enemyCanAttackNow && !weCanAttackFirst;
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