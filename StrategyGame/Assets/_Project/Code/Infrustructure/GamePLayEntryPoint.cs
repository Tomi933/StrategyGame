using Assets._Project.Code.UI;
using System.Collections;
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
            BotAI.Init(GridManager);
            TurnManager.Init(BotAI);

            EnemySpawner.SpawnEnemies(GridManager.GetTopCells());

            yield return null;

            //Чекаємо поки гравець нероставить всіх units
            yield return new WaitUntil(() => GameUIManager.IsPlacemantEnded);
            Debug.Log("GameUIManager.IsPlacemantEnded");

            yield return null;

            while (true)
            {
                Debug.Log("GameUIManager.IsPlayerPerformAction");
                yield return new WaitUntil(() => GameUIManager.IsPlayerPerformAction);

                TurnManager.EndPlayerTurn();
                GameUIManager.ClearPlayerPerformAction();

                Debug.Log("TurnManager.EndPlayerTurn()");

                yield return new WaitForSeconds(1);
            }
        }

        private void OnDestroy()
        {
            ExitButton.onClick.RemoveAllListeners();
        }

    }
}
