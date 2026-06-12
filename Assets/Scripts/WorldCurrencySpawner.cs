using UnityEngine;

// Gestioneaza banii din joc:
//  - Gold coin (1 gold)      : cade din FIECARE inamic ucis, la pozitia inamicului
//  - Platinum (30 gold)      : sansa 1 din 6 per kill, apare in zona jucatorului
//  - Money bag (100 gold)    : sansa 1 din 30 per kill, apare in zona jucatorului
// Toate sunt adunate in contorul "Gold:" prin CollectiblePickup -> GameManager.AddGold.
public class WorldCurrencySpawner : MonoBehaviour
{
    public static WorldCurrencySpawner Instance { get; private set; }

    public GameObject goldCoinPrefab;
    public GameObject platinumCoinPrefab;
    public GameObject moneyBagPrefab;
    public Transform player;

    [Header("Valori")]
    public int goldCoinValue = 1;
    public int platinumValue = 30;
    public int moneyBagValue = 100;

    [Header("Sanse per kill")]
    [Range(0f, 1f)] public float platinumChance = 1f / 6f;
    [Range(0f, 1f)] public float moneyBagChance = 1f / 30f;
    public float dropRadiusAroundPlayer = 3f;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    // Apelat de Enemy.Die() la fiecare kill.
    public static void NotifyKill(Vector3 enemyPosition)
    {
        if (Instance != null)
            Instance.HandleKillDrops(enemyPosition);
    }

    void HandleKillDrops(Vector3 enemyPosition)
    {
        // 1 gold coin din fiecare inamic, exact unde a murit
        SpawnWithValue(goldCoinPrefab, enemyPosition, goldCoinValue);

        // Platinum: sansa 1 din 6, in zona jucatorului
        if (Random.value < platinumChance)
            SpawnWithValue(platinumCoinPrefab, RandomNearPlayer(enemyPosition), platinumValue);

        // Money bag: sansa 1 din 30, in zona jucatorului
        if (Random.value < moneyBagChance)
            SpawnWithValue(moneyBagPrefab, RandomNearPlayer(enemyPosition), moneyBagValue);
    }

    Vector3 RandomNearPlayer(Vector3 fallback)
    {
        if (player == null) return fallback;
        Vector2 offset = Random.insideUnitCircle * dropRadiusAroundPlayer;
        return player.position + new Vector3(offset.x, offset.y, 0f);
    }

    void SpawnWithValue(GameObject prefab, Vector3 position, int value)
    {
        if (prefab == null) return;

        GameObject drop = Instantiate(prefab, position, Quaternion.identity);
        CollectiblePickup pickup = drop.GetComponent<CollectiblePickup>();
        if (pickup != null)
        {
            pickup.kind  = CollectibleKind.Gold;
            pickup.value = value;
        }
    }
}
