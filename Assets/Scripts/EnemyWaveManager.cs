using UnityEngine;
using CelikenVP;

// Spawn central de inamici cu reguli pe minute:
//  - min 0-2: doar lilieci (Enemy normal, 13 hp)
//  - min 2+:  lilieci + Vampires2 (25 hp), la random
//  - min 3+:  de 2x mai multi din ambele tipuri, la random
//  - cap GLOBAL de 20 inamici simultan (minibosii NU se numara aici)
// Spawneaza periodic + in valuri la fiecare waveInterval.
public class EnemyWaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;   // liliacul normal (13 hp)
    public Transform player;

    [Header("Wave settings")]
    public float waveInterval     = 10f;
    public int   enemiesPerWave   = 10;
    public float minSpawnDistance = 8f;
    public float maxSpawnDistance = 12f;
    public float spawnGap         = 0.1f;

    public const int MaxEnemies = 20; // minibosii nu se numara

    private float waveTimer;

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }
        if (enemyPrefab == null) return;

        waveTimer += Time.deltaTime;
        if (waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            StartCoroutine(SpawnWave());
        }
    }

    int CurrentMinute()
    {
        return Mathf.FloorToInt(Time.timeSinceLevelLoad / 60f);
    }

    public static int CountEnemies()
    {
        return Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
    }

    // Scalarea inamicilor: HP/speed cresc cu timpul, accelerat de stat-ul Curse.
    // Trebuie apelat imediat dupa Instantiate (inainte ca Enemy.Start sa citeasca maxHealth).
    public static void ApplyScaling(Enemy e)
    {
        if (e == null) return;
        int minute = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60f);
        float curse = PlayerStatsRuntime.GetMultiplier(StatType.Curse); // 1.0 = 100%

        float hpFactor    = (1f + minute * 0.15f) * curse * GameDifficulty.HealthMultiplier;
        float speedFactor = (1f + minute * 0.03f) * Mathf.Min(curse, 1.5f); // viteza creste mai lent

        e.maxHealth = Mathf.Max(1, Mathf.RoundToInt(e.maxHealth * hpFactor));
        e.speed    *= speedFactor;
    }

    System.Collections.IEnumerator SpawnWave()
    {
        int minute = CurrentMinute();
        // min 3+ => de 2x mai multi
        int count = enemiesPerWave * (minute >= 3 ? 2 : 1);

        for (int i = 0; i < count; i++)
        {
            if (CountEnemies() >= MaxEnemies) yield break; // cap global
            SpawnOneEnemy(minute);
            if (spawnGap > 0f) yield return new WaitForSeconds(spawnGap);
        }
    }

    void SpawnOneEnemy(int minute)
    {
        if (player == null) return;

        Vector2 dir  = Random.insideUnitCircle.normalized;
        float   dist = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector2 pos  = (Vector2)player.position + dir * dist;

        // min 2+ : 50% sansa de Vampires2 (25 hp)
        bool spawnVampire2 = minute >= 2 && Random.value < 0.5f;

        GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();

        if (spawnVampire2)
        {
            // Transforma liliacul intr-un inamic Vampires2: HP 25 + animatie proprie
            if (enemy != null) enemy.maxHealth = 25;
            go.transform.localScale = Vector3.one * 1.3f; // putin mai mare
            if (go.GetComponent<EnemyVampireAnimator>() == null)
                go.AddComponent<EnemyVampireAnimator>();
        }

        ApplyScaling(enemy); // scalare pe timp + Curse
    }
}
