using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnRate = 2f;
    public float spawnRadius = 10f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;
        Vector2 randomPos = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;
        Instantiate(enemyPrefab, randomPos, Quaternion.identity);
    }
}
