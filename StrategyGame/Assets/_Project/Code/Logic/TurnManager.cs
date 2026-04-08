using Assets._Project.Code.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameUIManager gameUI;
    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    public bool isPlayerTurn = true;

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        gameUI.EnablePlayerControls(false);

        foreach (var enemy in enemyUnits)
        {
            if (enemy.currentHP <= 0) continue;

            // Рух
            var moveCells = enemy.GetAvailableCellsFor(gameUI.GridManager, UnitMoveType.Cross);
            if (moveCells.Count > 0)
            {
                var targetCell = moveCells[Random.Range(0, moveCells.Count)];
                enemy.MoveTo(targetCell);
                yield return new WaitForSeconds(0.3f);
            }

            // Атака
            var attackTargets = enemy.GetAttackTargets2(gameUI.GridManager, playerUnits);
            if (attackTargets.Count > 0)
            {
                var target = attackTargets[Random.Range(0, attackTargets.Count)];
                enemy.Attack(target);
                yield return new WaitForSeconds(0.3f);
            }
        }

        // Повертаємо хід гравцю
        isPlayerTurn = true;
        gameUI.EnablePlayerControls(true);
    }
}