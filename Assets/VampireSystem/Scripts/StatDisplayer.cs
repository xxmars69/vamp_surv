
using System;
using System.Collections.Generic;
using UnityEngine;


namespace CelikenVP
{
    public class StatDisplayer : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO playerStats;

        [SerializeField] private Dictionary<StatType, LineStatRow> lineStat = new Dictionary<StatType, LineStatRow>();

        // Start is called before the first frame update
        void Start()
        {
            UpdateStats();
            PlayerStatsSO.Stat.OnStatUpdated += StatDisplayer_OnStatUpdated;
        }

        private void StatDisplayer_OnStatUpdated(object sender, StatType e)
        {
            UpdateStat(e);
        }

        void UpdateStats()
        {
            foreach (var stat in lineStat.Keys)
            {
                lineStat[stat].UpdateStatName(stat.ToString());
                UpdateStat(stat);
            }
        }

        void UpdateStat(StatType stat)
        {
            lineStat[stat].UpdateStatValue(playerStats.GetStat(stat).CurrentValue, playerStats.GetStat(stat).StackingMode);
        }
    }
}
