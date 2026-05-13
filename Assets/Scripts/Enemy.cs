using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    private Transform player;
    public GameObject xpGemPrefab;
    public GameObject deathEffectPrefab;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        // Miscare spre jucator
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        // Oglindire (Flip) in functie de pozitia jucatorului
        if (spriteRenderer != null)
        {
            if (player.position.x < transform.position.x)
                spriteRenderer.flipX = true; // Se uita spre stanga (depinde de cum e desenat sprite-ul original)
            else
                spriteRenderer.flipX = false; // Se uita spre dreapta
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerController>().TakeDamage(1);
            Die();
        }
    }

    public void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (xpGemPrefab != null)
        {
            Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
