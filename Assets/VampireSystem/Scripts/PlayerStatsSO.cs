
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

            [SerializeField] public StatType type;
            [SerializeField] public StackingMode stacking;
            [SerializeField] public float baseValue = 0f;
            [SerializeField] public float maxValue = -1f;
            [SerializeField] public float currentValue;

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

            public Stat()
            {
            }

            private Stat(StatType statType, StackingMode stackingMode, float baseStatValue, float maxStatValue)
            {
                type = statType;
                stacking = stackingMode;
                baseValue = baseStatValue;
                maxValue = maxStatValue;
                currentValue = baseStatValue;
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

            public static Stat CreateDefault(StatType statType, StackingMode stackingMode, float baseStatValue, float maxStatValue)
            {
                return new Stat(statType, stackingMode, baseStatValue, maxStatValue);
            }
        }

        [SerializeField] private Dictionary<StatType, Stat> stats = new();
        [SerializeField] private Dictionary<ObjectSO, int> levelObject = new();

        public Dictionary<StatType, Stat> Stats { get { return stats; } }
        public Dictionary<ObjectSO, int> LevelObject { get { return levelObject; } }

        public bool AddItem(ObjectSO item)
        {
            EnsureStats();
            if (levelObject.ContainsKey(item))
                levelObject[item]++;
            else
                levelObject.Add(item, 1);
            levelObject[item] = Mathf.Clamp(levelObject[item], 0, item.objectMaxLevel);
            foreach (var upgrade in item.upgrades)
            {
                GetStat(upgrade.StatType).AddUpgrade(upgrade);
            }
            return levelObject[item] == item.objectMaxLevel;
        }

        public Stat GetStat(StatType type)
        {
            EnsureStats();
            if (!stats.ContainsKey(type))
                stats[type] = CreateDefaultStat(type);
            return stats[type];
        }

        public bool TryGetStat(StatType type, out Stat stat)
        {
            EnsureStats();
            return stats.TryGetValue(type, out stat);
        }

        public int GetItemLevel(ObjectSO item)
        {
            if (levelObject.ContainsKey(item))
                return levelObject[item];
            return 0;
        }

        public void ClearObjects()
        {
            EnsureStats();
            levelObject.Clear();
            foreach (Stat stat in stats.Values)
            {
                stat.ClearStatUpgrades();
            }
        }

        private void EnsureStats()
        {
            stats ??= new Dictionary<StatType, Stat>();
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                if (!stats.ContainsKey(type))
                    stats[type] = CreateDefaultStat(type);
            }
        }

        private Stat CreateDefaultStat(StatType type)
        {
            return Stat.CreateDefault(type, GetDefaultStacking(type), GetDefaultBaseValue(type), GetDefaultMaxValue(type));
        }

        private StackingMode GetDefaultStacking(StatType type)
        {
            return type switch
            {
                StatType.Armor => StackingMode.AdditiveInt,
                StatType.MaxHealth => StackingMode.AdditiveInt,
                StatType.Recovery => StackingMode.Additive,
                StatType.Cooldown => StackingMode.AdditiveInt,
                StatType.Amount => StackingMode.AdditiveInt,
                StatType.Magnet => StackingMode.AdditiveInt,
                _ => StackingMode.MultiplicativeInt,
            };
        }

        private float GetDefaultBaseValue(StatType type)
        {
            return type switch
            {
                StatType.Armor => 0f,
                StatType.MaxHealth => 6f,
                StatType.Recovery => 0f,
                StatType.Cooldown => 0f,
                StatType.Amount => 1f,
                StatType.Magnet => 30f,
                _ => 100f,
            };
        }

        private float GetDefaultMaxValue(StatType type)
        {
            return type switch
            {
                StatType.Cooldown => 90f,
                StatType.Amount => 10f,
                StatType.Might => 1000f,
                StatType.Area => 1000f,
                StatType.Speed => 500f,
                StatType.Duration => 500f,
                _ => -1f,
            };
        }
    }
}
