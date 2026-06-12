
using UnityEngine;

namespace CelikenVP
{
    // [Serializable] e obligatoriu ca Unity sa salveze/incarce lista de upgrades din .asset;
    // fara el, item.upgrades era mereu gol la runtime si niciun item nu aplica statul.
    [System.Serializable]
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

