using UnityEngine;

namespace Assets._Project.Code.Infrustructure
{
    public class GameEntryPoint : MonoBehaviour
    {
        private void Awake()
        {
            GlobalServices.Initialize();

            GlobalServices.SceneLoader.LoadScene("Menu");
        }
    }
}