using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnData> enemies = new List<EnemySpawnData>();

    public void SpawnEnemies(List<Cell> availableCells)
    {
        foreach (var enemy in enemies)
        {
            for (int i = 0; i < enemy.Count; i++)
            {
                if (availableCells.Count == 0) return;

                int cellIndex = Random.Range(0, availableCells.Count);
                Cell cell = availableCells[cellIndex];

                var unitGO = Instantiate(enemy.UnitConfig.Prefab);


                unitGO.transform.position = cell.transform.position;

                
                unitGO.transform.SetParent(cell.transform, true);
                unitGO.transform.localScale = Vector3.one;

                var unit = unitGO.GetComponent<Unit>();
                unit.Init(cell, enemy.UnitConfig);
                unit.team = Team.Enemy;

                cell.isOccupied = true;

                availableCells.RemoveAt(cellIndex);
            }
        }
    }
}