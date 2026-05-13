
using UnityEngine;

namespace CelikenVP
{
    public class Upgrade
    {
        [SerializeField] private StatType type;
        [SerializeField] private StackingMode stacking;
        [SerializeField] private float upgradeValue;

        public StatType StatType { get { return type; } }
        public StackingMode Stacking { get { return stacking; } }
        public float UpgradeValue { get { return upgradeValue; } }
    }
}

