
using System;
using System.Collections.Generic;
using UnityEngine;


namespace CelikenVP
{
    public class StatDisplayer : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO playerStats;

        [SerializeField] private Dictionary<StatType, LineStatRow> lineStat = new Dictionary<StatType, LineStatRow>();
        [SerializeField] private Transform parentContent;
        [SerializeField] private GameObject pfStatLine;
        [SerializeField] private List<StatType> statsToShow = new()
        {
            StatType.Might,
            StatType.Armor,
            StatType.Cooldown,
            StatType.Area,
            StatType.Amount,
            StatType.MoveSpeed,
            StatType.Magnet,
            StatType.Luck,
            StatType.Growth,
            StatType.Greed
        };

        // Start is called before the first frame update
        void Start()
        {
            BuildRuntimeRowsIfNeeded();
            UpdateStats();
            PlayerStatsSO.Stat.OnStatUpdated += StatDisplayer_OnStatUpdated;
        }

        void OnDestroy()
        {
            PlayerStatsSO.Stat.OnStatUpdated -= StatDisplayer_OnStatUpdated;
        }

        private void StatDisplayer_OnStatUpdated(object sender, StatType e)
        {
            UpdateStat(e);
        }

        void UpdateStats()
        {
            if (playerStats == null)
                return;

            foreach (var stat in lineStat.Keys)
            {
                lineStat[stat].UpdateStatName(stat.ToString());
                UpdateStat(stat);
            }
        }

        void UpdateStat(StatType stat)
        {
            if (playerStats == null || !lineStat.ContainsKey(stat))
                return;

            if (!playerStats.TryGetStat(stat, out PlayerStatsSO.Stat statValue))
                return;

            lineStat[stat].UpdateStatValue(statValue.CurrentValue, statValue.StackingMode);
        }

        void BuildRuntimeRowsIfNeeded()
        {
            if (lineStat.Count > 0 || parentContent == null || pfStatLine == null)
                return;

            foreach (StatType stat in statsToShow)
            {
                GameObject row = Instantiate(pfStatLine, parentContent);
                LineStatRow line = row.GetComponent<LineStatRow>();
                if (line != null)
                    lineStat.Add(stat, line);
            }
        }
    }
}
