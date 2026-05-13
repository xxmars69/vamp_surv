using UnityEngine;

public class XPGem : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Transform player;
    private bool isFollowing = false;

    void Update()
    {
        if (isFollowing && player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (!isFollowing)
            {
                player = col.transform;
                isFollowing = true;
            }
            else if (Vector2.Distance(transform.position, player.position) < 0.5f)
            {
                GameManager.Instance.AddXP();
                Destroy(gameObject);
            }
        }
    }
}
