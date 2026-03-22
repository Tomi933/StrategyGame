using Assets._Project.Code.Configs.Units;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Code.UI
{
    public class SpawnUnitButton : MonoBehaviour
    {
        public Button Button;
        public Image UnitImage;
        public RectTransform HighlightFrame;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI UnitCountText;
        public UnitConfigSO UnitConfig;
        public int UnitCount = 2;
    }
}
