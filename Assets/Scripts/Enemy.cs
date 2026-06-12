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

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.15f;
    private Vector2 knockbackVelocity;
    private float knockbackTimer;

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

        // Cat timp e in knockback, e impins inapoi si nu urmareste jucatorul
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);
            return;
        }

        // Miscare spre jucator (cu debuff de incetinire daca e activ)
        float slow = GameManager.Instance != null ? GameManager.Instance.EnemySlowMultiplier : 1f;
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * slow * Time.deltaTime);

        // Oglindire (Flip) in functie de pozitia jucatorului
        if (spriteRenderer != null)
        {
            if (player.position.x < transform.position.x)
                spriteRenderer.flipX = true; // Se uita spre stanga (depinde de cum e desenat sprite-ul original)
            else
                spriteRenderer.flipX = false; // Se uita spre dreapta
        }
    }

    // Impinge inamicul intr-o directie pentru o scurta durata
    public void Knockback(Vector2 direction)
    {
        knockbackVelocity = direction.normalized * knockbackForce;
        knockbackTimer = knockbackDuration;
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

        ShieldSystem.RegisterKill();
        WorldCurrencySpawner.NotifyKill(transform.position);

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
        // Knockback dinspre jucator (sursa loviturilor)
        if (player != null) TakeDamage(damage, player.position);
        else TakeDamage(damage, transform.position);
    }

    public void TakeDamage(int damage, Vector2 hitFrom)
    {
        currentHealth -= damage;
        DamageNumber.Spawn(transform.position, damage);

        Vector2 dir = (Vector2)transform.position - hitFrom;
        if (dir.sqrMagnitude < 0.001f) dir = Random.insideUnitCircle;
        Knockback(dir);

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
