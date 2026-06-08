using CelikenVP;
using UnityEngine;

public enum CollectibleKind
{
    Experience,
    Gold
}

public class CollectiblePickup : MonoBehaviour
{
    public CollectibleKind kind = CollectibleKind.Experience;
    public int value = 1;
    public float moveSpeed = 8f;
    public float pickupDistance = 0.35f;

    private Transform player;

    void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
                return;
            player = playerObject.transform;
        }

        float magnetRadius = PlayerStatsRuntime.GetPercentStat(StatType.Magnet, 30f) / 10f;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= magnetRadius)
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        if (distance <= pickupDistance)
            Collect();
    }

    void Collect()
    {
        if (GameManager.Instance != null)
        {
            if (kind == CollectibleKind.Experience)
                GameManager.Instance.AddXP(value);
            else
                GameManager.Instance.AddGold(value);
        }

        Destroy(gameObject);
    }
}
