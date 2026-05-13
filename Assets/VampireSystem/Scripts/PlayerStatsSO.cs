
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CelikenVP
{
    public enum StatType
    {
        Might,
        Armor,
        MaxHealth,
        Recovery,
        Cooldown,
        Area,
        Speed,
        Duration,
        Amount,
        MoveSpeed,
        Magnet,
        Luck,
        Growth,
        Greed,
        Curse,
        Charm,
    }

    public enum StackingMode
    {
        Additive,
        AdditiveInt,
        Multiplicative,
        MultiplicativeInt,
    }

    [CreateAssetMenu(menuName = "Player Stats")]
    public class PlayerStatsSO : ScriptableObject
    {
        public class Stat
        {
            public static event EventHandler<StatType> OnStatUpdated;

            [SerializeField] private StatType type;
            [SerializeField] private StackingMode stacking;
            [SerializeField] private float baseValue = 0f;
            [SerializeField] private float maxValue = -1f;
            [SerializeField] private float currentValue;

            private List<Upgrade> listUpgrade = new();

            public StatType StatType { get { return type; } }
            public StackingMode StackingMode { get { return stacking; } }
            public float BaseValue { get { return baseValue; } }
            public float MaxValue { get { return maxValue; } }
            public float CurrentValue
            {
                get
                {
                    var val = maxValue < 0f ? currentValue : Mathf.Clamp(currentValue, baseValue, maxValue);
                    if (stacking == StackingMode.MultiplicativeInt || stacking == StackingMode.AdditiveInt)
                        return Mathf.CeilToInt(val);
                    return val;
                }
            }

            public void ComputeValue()
            {
                float additive = 0f;
                float multiplicative = 1f;
                if (listUpgrade != null)
                    foreach (var upgrade in listUpgrade)
                    {
                        if (upgrade.Stacking == StackingMode.Additive)
                            additive += upgrade.UpgradeValue;
                        if (upgrade.Stacking == StackingMode.Multiplicative)
                            multiplicative *= upgrade.UpgradeValue;
                    }
                currentValue = (baseValue + additive) * multiplicative;
                OnStatUpdated?.Invoke(this, type);
            }

            public void AddUpgrade(Upgrade upgrade)
            {
                listUpgrade ??= new List<Upgrade>();
                listUpgrade.Add(upgrade);
                ComputeValue();
            }

            public void ClearStatUpgrades()
            {
                listUpgrade?.Clear();
                ComputeValue();
            }
        }

        [SerializeField] private Dictionary<StatType, Stat> stats = new();
        [SerializeField] private Dictionary<ObjectSO, int> levelObject = new();

        public Dictionary<StatType, Stat> Stats { get { return stats; } }
        public Dictionary<ObjectSO, int> LevelObject { get { return levelObject; } }

        public bool AddItem(ObjectSO item)
        {
            if (levelObject.ContainsKey(item))
                levelObject[item]++;
            else
                levelObject.Add(item, 1);
            levelObject[item] = Mathf.Clamp(levelObject[item], 0, item.objectMaxLevel);
            foreach (var upgrade in item.upgrades)
            {
                stats[upgrade.StatType].AddUpgrade(upgrade);
            }
            return levelObject[item] == item.objectMaxLevel;
        }

        public Stat GetStat(StatType type)
        {
            return stats[type];
        }

        public int GetItemLevel(ObjectSO item)
        {
            if (levelObject.ContainsKey(item))
                return levelObject[item];
            return 0;
        }

        public void ClearObjects()
        {
            levelObject.Clear();
            foreach (Stat stat in stats.Values)
            {
                stat.ClearStatUpgrades();
            }
        }
    }
}
