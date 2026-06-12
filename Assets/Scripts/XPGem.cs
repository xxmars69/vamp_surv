using UnityEngine;
using CelikenVP;

public class XPGem : MonoBehaviour
{
    public int xpValue = 10;
    public float moveSpeed = 5f;
    private Transform player;
    private bool isFollowing = false;

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        // Magnet: incepe sa zboare spre jucator cand intra in raza Magnet
        if (!isFollowing)
        {
            float magnetRadius = PlayerStatsRuntime.GetPercentStat(StatType.Magnet, 30f) / 10f;
            if (dist <= magnetRadius) isFollowing = true;
        }

        if (isFollowing)
        {
            // Accelereaza pe masura ce se apropie (efect VS)
            float speed = moveSpeed + (1f / Mathf.Max(0.3f, dist)) * 3f;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            if (dist < 0.4f)
            {
                if (GameManager.Instance != null) GameManager.Instance.AddXP(xpValue);
                SoundManager.Play(SoundManager.Sfx.Pickup);
                Destroy(gameObject);
            }
        }
    }
}
