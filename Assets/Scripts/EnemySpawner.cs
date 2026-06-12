using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnRate = 2f;
    public float spawnRadius = 10f;
    private float timer;

    void Start()
    {
        // Auto-instalare wave manager pe acelasi GameObject, daca nu exista deja
        EnemyWaveManager wm = GetComponent<EnemyWaveManager>();
        if (wm == null) wm = gameObject.AddComponent<EnemyWaveManager>();
        if (wm.enemyPrefab == null) wm.enemyPrefab = enemyPrefab;
        if (wm.player == null)      wm.player      = player;

        // Spawner de miniboss pe acelasi GameObject
        MinibossSpawner ms = GetComponent<MinibossSpawner>();
        if (ms == null) ms = gameObject.AddComponent<MinibossSpawner>();
        if (ms.player == null) ms.player = player;

        // Spawner de Ham (heal pickup) la fiecare 30s
        HamSpawner hs = GetComponent<HamSpawner>();
        if (hs == null) hs = gameObject.AddComponent<HamSpawner>();
        if (hs.player == null) hs.player = player;
    }

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
        if (EnemyWaveManager.CountEnemies() >= EnemyWaveManager.MaxEnemies) return; // cap global

        Vector2 randomPos = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;

        int minute = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60f);
        bool vampire2 = minute >= 2 && Random.value < 0.5f;

        GameObject go = Instantiate(enemyPrefab, randomPos, Quaternion.identity);
        if (vampire2)
        {
            Enemy e = go.GetComponent<Enemy>();
            if (e != null) e.maxHealth = 25;
            go.transform.localScale = Vector3.one * 1.3f;
            if (go.GetComponent<EnemyVampireAnimator>() == null)
                go.AddComponent<EnemyVampireAnimator>();
        }
    }
}
