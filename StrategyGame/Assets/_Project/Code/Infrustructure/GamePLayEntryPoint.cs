using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Code.Infrustructure
{
    public class GamePLayEntryPoint: MonoBehaviour
    {
        public Button ExitButton;

        private void Awake()
        {
            ExitButton.onClick.AddListener(() =>
            {
                GlobalServices.SceneLoader.LoadScene("Menu");
            });


            //створити мапу
            //ротавити фігури
        }

        private void OnDestroy()
        {
            ExitButton.onClick.RemoveAllListeners();
        }

    }
}
