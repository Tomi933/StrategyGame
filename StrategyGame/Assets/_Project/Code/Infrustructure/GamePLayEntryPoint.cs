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
            EnemySpawner.SpawnEnemies(GridManager.GetTopCells());

            yield return null;

            

        }

        private void OnDestroy()
        {
            ExitButton.onClick.RemoveAllListeners();
        }

    }
}
