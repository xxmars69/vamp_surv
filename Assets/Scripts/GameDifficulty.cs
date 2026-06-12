// Selectie de dificultate (adaptarea "Level Select" - avem o singura harta,
// deci stage-urile sunt nivele de dificultate care scaleaza inamicii).
public static class GameDifficulty
{
    public enum Level { Easy, Normal, Hard }

    public static Level Selected = Level.Normal;

    // Multiplicator HP inamici dupa dificultate
    public static float HealthMultiplier
    {
        get
        {
            switch (Selected)
            {
                case Level.Easy: return 0.7f;
                case Level.Hard: return 1.6f;
                default:         return 1f;
            }
        }
    }

    public static string Name(Level l)
    {
        switch (l)
        {
            case Level.Easy: return "Usor";
            case Level.Hard: return "Greu";
            default:         return "Normal";
        }
    }
}
