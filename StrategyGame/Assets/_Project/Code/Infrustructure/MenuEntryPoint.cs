using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Code.Infrustructure
{
    public class MenuEntryPoint : MonoBehaviour
    {
        public Button PlayButton;

        private void Awake()
        {
            PlayButton.onClick.AddListener(() =>{
                GlobalServices.SceneLoader.LoadScene("Game");
            });
        }

        private void OnDestroy()
        {
            PlayButton.onClick.RemoveAllListeners();
        }
    }
}