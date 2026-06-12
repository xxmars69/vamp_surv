using UnityEngine;

// Profil per personaj: stats afisate in meniu + bonusuri aplicate la inceputul jocului.
// Diferentele intre Hero si Matilda sunt mici (nu cu mult), dar fiecare are
// un punct forte: Hero damage, Matilda recovery + viteza.
public static class CharacterProfile
{
    public struct Profile
    {
        public string name;
        public int    hearts;
        public int    speedPercent;
        public int    damage;          // damage afisat in stats si folosit ca damage real
        public int    luckPercent;
        public float  recoveryAmount;
        public float  recoveryInterval;
        public int    shieldKills;
        public float  attackInterval; // secunde intre atacuri (Hero=glonte, ceilalti=melee)
    }

    public static Profile GetProfile(CharacterSelectionData.CharacterType type)
    {
        // Profil de baza + bonusurile permanente cumparate in meniu (MetaProgress)
        Profile p = BaseProfile(type);
        p.damage       += MetaProgress.DamageBonus;
        p.hearts       += MetaProgress.HealthBonus;
        p.speedPercent += MetaProgress.SpeedBonus;
        return p;
    }

    static Profile BaseProfile(CharacterSelectionData.CharacterType type)
    {
        switch (type)
        {
            case CharacterSelectionData.CharacterType.Matilda:
                return new Profile
                {
                    name = "Matilda", hearts = 4, speedPercent = 7, damage = 11,
                    luckPercent = 5, recoveryAmount = 1f, recoveryInterval = 12f, shieldKills = 5,
                    attackInterval = 1.4f,
                };

            case CharacterSelectionData.CharacterType.Countess_Vampire:
                return new Profile
                {
                    name = "Countess", hearts = 5, speedPercent = 4, damage = 13,
                    luckPercent = 3, recoveryAmount = 1f, recoveryInterval = 8f, shieldKills = 4,
                    attackInterval = 1.6f,
                };

            case CharacterSelectionData.CharacterType.Dracula:
                return new Profile
                {
                    name = "Dracula", hearts = 4, speedPercent = 4, damage = 15,
                    luckPercent = 2, recoveryAmount = 1f, recoveryInterval = 20f, shieldKills = 6,
                    attackInterval = 1.8f,
                };

            case CharacterSelectionData.CharacterType.Vampire_Girl:
                // Glass cannon: o singura inima dar luck mare si atacuri rapide
                return new Profile
                {
                    name = "Vampire Girl", hearts = 1, speedPercent = 9, damage = 7,
                    luckPercent = 10, recoveryAmount = 1f, recoveryInterval = 14f, shieldKills = 5,
                    attackInterval = 1.2f,
                };

            default: // Hero
                return new Profile
                {
                    name = "Hero", hearts = 3, speedPercent = 5, damage = 5,
                    luckPercent = 0, recoveryAmount = 1f, recoveryInterval = 15f, shieldKills = 5,
                    attackInterval = 0.6f,
                };
        }
    }

    public static Profile Current => GetProfile(CharacterSelectionData.Selected);

    // Multiplicator de damage aplicat in joc (Hero are baseline 1.2x, Matilda 0.8x)
    public static float DamageMultiplier(CharacterSelectionData.CharacterType type)
    {
        return GetProfile(type).damage / 10f;
    }
}
