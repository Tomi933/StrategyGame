using Assets._Project.Code.Configs.Units;
using Assets._Project.Code.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Code.Infrustructure
{
    public class GamePLayEntryPoint: MonoBehaviour
    {
        public Button ExitButton;
        public GridManager GridManager;
        public EnemySpawner EnemySpawner;
        public GameUIManager GameUIManager;
        public BotAI BotAI;
        public TurnManager TurnManager;
        public GameOverUI GameOverUI;

        private void Awake()
        {
            ExitButton.onClick.AddListener(() => {
                GlobalServices.SceneLoader.LoadScene("Menu");
            });

            StartCoroutine(BeginGame());
        }

        private IEnumerator BeginGame()
        {
            GridManager.GenerateGrid();
            GameUIManager.Init(GridManager);
            BotDifficulty difficulty = (BotDifficulty)PlayerPrefs.GetInt("Difficulty", (int)BotDifficulty.Medium);
            Debug.Log("BotDifficulty difficulty = (BotDifficulty)PlayerPrefs.GetInt(\"Difficulty\", (int)BotDifficulty.Medium);" + difficulty);
            BotAI.Init(GridManager, difficulty);
            TurnManager.Init(BotAI, GameUIManager);

            EnemySpawner.SpawnEnemies(GridManager.GetTopCells());
            CheckBasesDeath();

            yield return null;

            //Чекаємо поки гравець нероставить всіх units
            yield return new WaitUntil(() => GameUIManager.IsPlacemantEnded);
            Debug.Log("GameUIManager.IsPlacemantEnded");

            yield return null;

            while (true)
            {
                yield return new WaitUntil(() => GameUIManager.IsPlayerPerformAction);
                GameUIManager.RefreshEnemyVisibility();
                GameUIManager.ClearPlayerPerformAction();
                TurnManager.EndPlayerTurn();
                yield return new WaitForSeconds(1);
            }
        }

        private void CheckBasesDeath()
        {
            var all = FindObjectsByType<Unit>(FindObjectsSortMode.None);

            foreach (var unit in all)
            {
                if (unit.Config.Behavior != UnitBehavior.Static) continue;

                unit.OnDiedEvent += () =>
                {
                    bool playerWon = unit.team == Team.Enemy;
                    GameOverUI.Show(playerWon);
                    StopAllCoroutines();
                };
            }
        }

        private void OnDestroy()
        {
            ExitButton.onClick.RemoveAllListeners();
        }
    }
}
