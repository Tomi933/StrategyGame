using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Code.Configs.Units
{
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "Configs/Units/UnitConfig")]
    public class UnitConfigSO : ScriptableObject
    {
        public string Name;
        public GameObject Prefab;
        public Sprite Sprite;
    }
}
