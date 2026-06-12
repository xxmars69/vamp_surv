using UnityEngine;

// Spawneaza un miniboss la fiecare minut, cu HP si damage scalat:
//  min 1: 150 hp / 1 inima
//  min 2: 250 hp / 2 inimi
//  min 3: 350 hp / 3 inimi
//  ...
//  min 6+: 650 hp / 6 inimi (max)
public class MinibossSpawner : MonoBehaviour
{
    public Transform player;
    public float minute = 60f;
    public float spawnDistance = 10f;

    private float timer;
    private int minutesElapsed;

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        timer += Time.deltaTime;
        if (timer >= minute)
        {
            timer = 0f;
            minutesElapsed++;
            SpawnMiniboss(minutesElapsed);
        }
    }

    void SpawnMiniboss(int minuteN)
    {
        int level = Mathf.Clamp(minuteN, 1, 6);

        int hp     = 150 + (level - 1) * 100;   // 150, 250, 350, 450, 550, 650
        // Damage la jucator e fix la 1 inima per hit, indiferent de minut
        int hearts = 1;

        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 pos = player.position + new Vector3(dir.x, dir.y, 0f) * spawnDistance;

        GameObject go = new GameObject("Miniboss_" + minuteN);
        go.transform.position = pos;
        Miniboss mb = go.AddComponent<Miniboss>();
        mb.maxHealth    = hp;
        mb.damageHearts = hearts;
    }
}
