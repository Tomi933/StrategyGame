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
            EnemySpawner.SpawnEnemies(GridManager.GetTopCells());
            BotAI.Init(GridManager, difficulty);
            BotAI.ApplyDifficultyMultipliers();
            TurnManager.Init(BotAI, GameUIManager);
            TurnManager.OnBotTurnEnded += OnBotTurnEnded;

            GlobalServices.AudioService.PlayMusic("GameplayMusic");

            yield return null;
            yield return new WaitUntil(() => GameUIManager.IsPlacemantEnded);

            GlobalServices.AudioService.PlayClip("EndSelection");

            yield return null;

            while (true)
            {
                yield return new WaitUntil(() => GameUIManager.IsPlayerPerformAction);
                GameUIManager.RefreshEnemyVisibility();
                GameUIManager.ClearPlayerPerformAction();
                if (CheckGameOver()) yield break;
                TurnManager.EndPlayerTurn();
                yield return new WaitForSeconds(1);
            }
        }

        private void OnBotTurnEnded()
        {
            if (CheckGameOver())
                TurnManager.OnBotTurnEnded -= OnBotTurnEnded;
        }

        private bool CheckGameOver()
        {
            var all = FindObjectsByType<Unit>(FindObjectsSortMode.None);

            bool hasPlayerBase = false;
            bool hasPlayerUnits = false;
            bool hasEnemyBase = false;

            foreach (var u in all)
            {
                if (u.team == Team.Player)
                {
                    if (u.Config.Behavior == UnitBehavior.Static) hasPlayerBase = true;
                    else hasPlayerUnits = true;
                }
                else if (u.team == Team.Enemy)
                {
                    if (u.Config.Behavior == UnitBehavior.Static) hasEnemyBase = true;
                }
            }

            if (!hasPlayerBase || !hasPlayerUnits)
            {
                GameOverUI.Show(false);
                StopAllCoroutines();
                GameUIManager.SetPlayerTurn(false);
                return true;
            }

            if (!hasEnemyBase)
            {
                GameOverUI.Show(true);
                StopAllCoroutines();
                GameUIManager.SetPlayerTurn(false);
                return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            ExitButton.onClick.RemoveAllListeners();
            if (TurnManager != null)
                TurnManager.OnBotTurnEnded -= OnBotTurnEnded;
        }
    }
}
