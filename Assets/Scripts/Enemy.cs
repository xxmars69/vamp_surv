using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int maxHealth = 1;
    private Transform player;
    public GameObject blueXpGemPrefab;
    public GameObject redXpGemPrefab;
    public GameObject deathEffectPrefab;
    private SpriteRenderer spriteRenderer;
    private int currentHealth;
    private static int totalEnemiesKilled;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
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
        HealthUI ui = Object.FindAnyObjectByType<HealthUI>();
        if (ui != null) ui.AddKill();

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        totalEnemiesKilled++;
        DropExperience();

        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    void DropExperience()
    {
        if (blueXpGemPrefab != null)
            Instantiate(blueXpGemPrefab, transform.position, Quaternion.identity);

        if (totalEnemiesKilled % 2 == 0 && redXpGemPrefab != null)
        {
            Vector3 offset = new Vector3(0.25f, 0.15f, 0f);
            Instantiate(redXpGemPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}
