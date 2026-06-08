using CelikenVP;
using UnityEngine;

public static class PlayerStatsRuntime
{
    public static PlayerStatsSO Stats
    {
        get
        {
            if (Player.Instance == null)
                return null;
            return Player.Instance.PlayerStats;
        }
    }

    public static float GetPercentStat(StatType statType, float fallbackPercent = 100f)
    {
        PlayerStatsSO stats = Stats;
        if (stats == null)
            return fallbackPercent;

        try
        {
            return stats.GetStat(statType).CurrentValue;
        }
        catch
        {
            return fallbackPercent;
        }
    }

    public static float GetMultiplier(StatType statType, float fallbackPercent = 100f)
    {
        return GetPercentStat(statType, fallbackPercent) / 100f;
    }

    public static int GetIntStat(StatType statType, int fallback = 0)
    {
        PlayerStatsSO stats = Stats;
        if (stats == null)
            return fallback;

        try
        {
            return Mathf.RoundToInt(stats.GetStat(statType).CurrentValue);
        }
        catch
        {
            return fallback;
        }
    }
}
