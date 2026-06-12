using UnityEngine;

// Meta-progresie persistenta (Partile 26 + 28): gold total salvat intre runde +
// upgrade-uri permanente cumparate in meniu. Foloseste PlayerPrefs.
public static class MetaProgress
{
    const string K_Gold   = "meta_total_gold";
    const string K_Damage = "meta_lvl_damage";
    const string K_Health = "meta_lvl_health";
    const string K_Speed  = "meta_lvl_speed";

    // Niveluri maxime
    public const int MaxDamage = 10;
    public const int MaxHealth = 5;
    public const int MaxSpeed  = 10;

    public static int TotalGold
    {
        get => PlayerPrefs.GetInt(K_Gold, 0);
        set { PlayerPrefs.SetInt(K_Gold, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static int DamageLevel => PlayerPrefs.GetInt(K_Damage, 0);
    public static int HealthLevel => PlayerPrefs.GetInt(K_Health, 0);
    public static int SpeedLevel  => PlayerPrefs.GetInt(K_Speed, 0);

    // Bonusuri aplicate la stats-urile personajului
    public static int DamageBonus => DamageLevel;      // +1 damage / nivel
    public static int HealthBonus => HealthLevel;      // +1 inima / nivel
    public static int SpeedBonus  => SpeedLevel;       // +1% viteza / nivel

    // Costul urmatorului nivel
    public static int DamageCost => 50 + DamageLevel * 50;
    public static int HealthCost => 80 + HealthLevel * 80;
    public static int SpeedCost  => 40 + SpeedLevel * 40;

    public static void BankRunGold(int runGold)
    {
        TotalGold += runGold;
    }

    public static bool BuyDamage()
    {
        if (DamageLevel >= MaxDamage || TotalGold < DamageCost) return false;
        TotalGold -= DamageCost;
        PlayerPrefs.SetInt(K_Damage, DamageLevel + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static bool BuyHealth()
    {
        if (HealthLevel >= MaxHealth || TotalGold < HealthCost) return false;
        TotalGold -= HealthCost;
        PlayerPrefs.SetInt(K_Health, HealthLevel + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static bool BuySpeed()
    {
        if (SpeedLevel >= MaxSpeed || TotalGold < SpeedCost) return false;
        TotalGold -= SpeedCost;
        PlayerPrefs.SetInt(K_Speed, SpeedLevel + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(K_Gold);
        PlayerPrefs.DeleteKey(K_Damage);
        PlayerPrefs.DeleteKey(K_Health);
        PlayerPrefs.DeleteKey(K_Speed);
        PlayerPrefs.Save();
    }
}
