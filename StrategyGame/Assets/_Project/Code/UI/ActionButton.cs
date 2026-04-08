using Assets._Project.Code.Configs.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Code.UI
{
    public class ActionButton : MonoBehaviour
    {
        public Button Button;
        public GameObject Highlight;
        public Image ModeImage;
        public ActionMode Mode;

        private GameUIManager _uiManager;

        public void Init(GameUIManager uiManager)
        {
            _uiManager = uiManager;

            Button.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            _uiManager.SetMode(Mode);
            Debug.Log("Mode: " + Mode);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetHighlight(bool active)
        {
            if (Highlight != null)
                Highlight.SetActive(active);
        }
    }
}