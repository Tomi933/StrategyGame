using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Code.Configs.Units
{
    public enum UnitBehavior { Static, Dynamic}

    [CreateAssetMenu(fileName = "UnitConfig", menuName = "Configs/Units/UnitConfig")]
    public class UnitConfigSO : ScriptableObject
    {
        [Header("Visual")]
        public string Name;
        public GameObject Prefab;
        public Sprite Sprite;

        [Header("Base")]
        public UnitMoveType MoveType;
        public UnitAttackType AttackType;
        public UnitBehavior Behavior;
        [Range(1, 10)] public int MoveRange;
        [Range(1f, 20f)] public float Health;

        [Header("Attack")]
        [Range(1, 20)] public float damage = 3;
        [Range(1f, 10f)] public int attackRange = 1;
    }
}
