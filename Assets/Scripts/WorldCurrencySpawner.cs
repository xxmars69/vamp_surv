using UnityEngine;

public class WorldCurrencySpawner : MonoBehaviour
{
    public GameObject goldCoinPrefab;
    public GameObject platinumCoinPrefab;
    public GameObject moneyBagPrefab;
    public Transform player;
    public float minSpawnDelay = 4f;
    public float maxSpawnDelay = 8f;
    public float spawnRadius = 12f;

    private float nextSpawnTime;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnCurrency();
            ScheduleNextSpawn();
        }
    }

    void SpawnCurrency()
    {
        if (player == null)
            return;

        GameObject prefab = PickCurrencyPrefab();
        if (prefab == null)
            return;

        Vector2 spawnPosition = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    GameObject PickCurrencyPrefab()
    {
        float roll = Random.value;
        if (roll < 0.03f && moneyBagPrefab != null)
            return moneyBagPrefab;
        if (roll < 0.18f && platinumCoinPrefab != null)
            return platinumCoinPrefab;
        return goldCoinPrefab;
    }

    void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }
}
