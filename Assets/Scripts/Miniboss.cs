using System.Collections;
using System.IO;
using UnityEngine;

// Miniboss: vampire mare cu animatii idle/run/attack/hurt/death.
// Spawnat de MinibossSpawner cu HP si damage scalat dupa minutul curent.
public class Miniboss : MonoBehaviour
{
    public const int CELL = 64;
    public const float PPU = 24f; // 64/24 ≈ 2.7 units inaltime -> putin mai mare decat jucatorul

    [Header("Stats - setate de spawner")]
    public int maxHealth = 150;
    public int damageHearts = 1; // FIX: 1 inima (2 HP) la fiecare hit, indiferent de minut

    [Header("Behaviour")]
    public float speed = 1.5f;  // 2x mai incet ca Enemy (care e 3)
    public float attackRange = 1.5f;
    public float attackInterval = 0.6f; // loveste mai des cand atinge jucatorul

    private int currentHealth;
    private Transform player;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    // Animatii (rand 0 = directia "jos" din strip-ul 4-direction)
    private Sprite[] idle, run, attack, hurt, death;
    private enum State { Idle, Run, Attack, Hurt, Dead }
    private State state = State.Idle;
    private Sprite[] current;
    private int frameIndex;
    private float frameTimer;
    private bool attacking, hurting, dying;
    private float attackTimer;

    private static readonly System.Collections.Generic.Dictionary<string, Texture2D> texCache = new();

    void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        // Kinematic = nu e aruncat de coliziuni; miscarea prin transform.position e curata
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.6f;
        // Trigger = nu impinge fizic jucatorul; damage-ul e prin verificare de distanta
        col.isTrigger = true;

        idle   = LoadStrip("Idle.png");
        run    = LoadStrip("Run.png");
        attack = LoadStrip("Attack.png");
        hurt   = LoadStrip("Hurt.png");
        death  = LoadStrip("Death.png");

        current = idle;
    }

    void Start()
    {
        currentHealth = maxHealth;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (dying)
        {
            AdvanceAnim(0.12f);
            return;
        }

        if (player == null) return;

        Vector2 to = (Vector2)(player.position - transform.position);
        float dist = to.magnitude;

        // Flip vizual dupa pozitia jucatorului
        if (Mathf.Abs(to.x) > 0.05f && sr != null) sr.flipX = to.x < 0f;

        attackTimer -= Time.deltaTime;

        if (dist > attackRange)
        {
            // Urmareste jucatorul (animatia de atac nu mai blocheaza miscarea)
            if (!attacking) SetState(State.Run);
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else
        {
            // In raza: loveste la fiecare attackInterval (0.6s), fara sa astepte animatia
            if (attackTimer <= 0f)
            {
                DoAttack();
                attackTimer = attackInterval;
            }
            else if (!attacking) SetState(State.Idle);
        }

        AdvanceAnim(attacking ? 0.06f : 0.12f);
    }

    void DoAttack()
    {
        SetState(State.Attack);
        attacking = true;

        if (player != null)
        {
            float d = Vector2.Distance(transform.position, player.position);
            if (d <= attackRange + 0.3f)
            {
                var pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.TakeDamage(damageHearts * 2);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (dying) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
        // Fara stare de Hurt care blocheaza - jucatorul loveste foarte des,
        // minibossul nu trebuie sa se opreasca din urmarire.
    }

    void Die()
    {
        dying = true;
        SetState(State.Dead);
        if (rb != null) rb.simulated = false;

        // Damage death effect dupa terminarea animatiei
        StartCoroutine(DestroyAfter(1.5f));

        // Drop XP gems mari (folosim acelasi sistem ca Enemy.cs)
        ShieldSystem.RegisterKill();
        WorldCurrencySpawner.NotifyKill(transform.position);

        HealthUI ui = Object.FindAnyObjectByType<HealthUI>();
        if (ui != null) ui.AddKill();
    }

    IEnumerator DestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }

    void SetState(State s)
    {
        if (state == s) return;
        state = s;
        frameIndex = 0;
        frameTimer = 0f;
        current = Pick(s);
    }

    Sprite[] Pick(State s)
    {
        switch (s)
        {
            case State.Run:    return run;
            case State.Attack: return attack;
            case State.Hurt:   return hurt;
            case State.Dead:   return death;
            default:           return idle;
        }
    }

    void AdvanceAnim(float frameDur)
    {
        if (current == null || current.Length == 0 || sr == null) return;
        frameTimer += Time.deltaTime;
        if (frameTimer >= frameDur)
        {
            frameTimer -= frameDur;
            frameIndex++;
            if (frameIndex >= current.Length)
            {
                if (state == State.Attack) attacking = false;
                if (state == State.Hurt)   hurting   = false;
                if (state == State.Dead) { frameIndex = current.Length - 1; return; }
                frameIndex = 0;
            }
        }
        sr.sprite = current[frameIndex];
    }

    // Strip 4-direction (rand 0 = jos), cadre 64x64
    Sprite[] LoadStrip(string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Sprites/Miniboss", fileName);
        if (!File.Exists(path)) return new Sprite[0];

        if (!texCache.TryGetValue(path, out var tex))
        {
            byte[] bytes = File.ReadAllBytes(path);
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            if (!tex.LoadImage(bytes)) return new Sprite[0];
            texCache[path] = tex;
        }

        int cols = tex.width / CELL;
        // Folosim ultimul rand (in textura origine jos-stanga, deci y=0 = primul rand din PNG-ul jos sus)
        // Pentru 4-direction packs, "down" e adesea primul rand din PNG (sus). Convertit la jos = ultimul.
        int rowY = tex.height - CELL;
        Sprite[] arr = new Sprite[cols];
        for (int i = 0; i < cols; i++)
        {
            Rect r = new Rect(i * CELL, rowY, CELL, CELL);
            arr[i] = Sprite.Create(tex, r, new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect);
        }
        return arr;
    }
}
