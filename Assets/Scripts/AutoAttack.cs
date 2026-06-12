using UnityEngine;
using CelikenVP;

public class AutoAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    [SerializeField] private float baseFireInterval = 1f;
    [SerializeField] private float spreadAngle = 12f;

    [Header("Matilda - atac cu coasa")]
    [SerializeField] private float matildaAttackInterval = 2f; // o data la 2 secunde
    [SerializeField] private float matildaScytheRange = 2f;      // raza coasei
    [SerializeField] private float matildaScytheOffset = 1f;     // cat de in fata loveste
    [SerializeField] private int matildaScytheDamage = 3;

    private float nextFireTime;
    private float nextMatildaAttack;
    private Animator anim;
    private PlayerController player;
    private MatildaAnimator matildaAnim;
    private VampireAnimator vampAnim;

    void Start()
    {
        anim        = GetComponent<Animator>();
        player      = GetComponent<PlayerController>();
        matildaAnim = GetComponent<MatildaAnimator>();

        // Aplicam intervalul de atac din profilul personajului
        float interval = CharacterProfile.Current.attackInterval;
        if (interval > 0f)
        {
            baseFireInterval      = interval;
            matildaAttackInterval = interval;
        }
    }

    // Evolutie: accelereaza ritmul de atac (multiplicator < 1 = mai rapid)
    public void BoostFireRate(float multiplier)
    {
        baseFireInterval      *= multiplier;
        matildaAttackInterval *= multiplier;
    }

    void Update()
    {
        if (player == null || player.currentHealth <= 0) return;

        // Matilda + vampirele: melee in directia de mers, cooldown separat
        if (player.IsMatilda || player.IsVampire)
        {
            if (Time.time >= nextMatildaAttack)
            {
                DoMeleeAttack();
                nextMatildaAttack = Time.time + matildaAttackInterval;
            }
            return;
        }

        float cooldownPercent = PlayerStatsRuntime.GetPercentStat(StatType.Cooldown, 0f);
        float fireInterval    = baseFireInterval * Mathf.Clamp01(1f - cooldownPercent / 100f);
        fireInterval          = Mathf.Max(0.08f, fireInterval);

        if (Time.time >= nextFireTime)
        {
            if (TryShoot())
                nextFireTime = Time.time + fireInterval;
        }
    }

    // Atac per personaj. Trigger animatie + damage cu mic delay = sincronizare cu loviturile.
    const float AttackSyncDelay = 0.2f; // delay intre animatie si damage (toate personajele)

    void DoMeleeAttack()
    {
        // Trigger animatie INAINTE de damage (sincronizare 0.2s)
        if (player.IsMatilda)
        {
            if (matildaAnim == null) matildaAnim = GetComponent<MatildaAnimator>();
            if (matildaAnim != null) matildaAnim.TriggerAttack();
        }
        else if (player.IsVampire)
        {
            if (vampAnim == null) vampAnim = GetComponent<VampireAnimator>();
            if (vampAnim != null) vampAnim.TriggerAttack();
        }

        float might   = PlayerStatsRuntime.GetMultiplier(StatType.Might);
        int   baseDmg = CharacterProfile.Current.damage;
        int damage    = Mathf.Max(1, Mathf.RoundToInt(baseDmg * might));

        var selected = CharacterSelectionData.Selected;

        // COUNTESS: proiectil cu auto-aim dupa delay-ul de sincronizare
        if (selected == CharacterSelectionData.CharacterType.Countess_Vampire)
        {
            Enemy target = FindNearestEnemy();
            if (target == null) return;
            StartCoroutine(CountessAttackSequence(target, damage));
            return;
        }

        // Restul personajelor: damage instant cu delay 0.2s (sincronizat cu loviturile)
        if (selected == CharacterSelectionData.CharacterType.Dracula)
        {
            StartCoroutine(DelayedSwordHit(damage, AttackSyncDelay));
            return;
        }

        if (selected == CharacterSelectionData.CharacterType.Vampire_Girl)
        {
            StartCoroutine(DelayedAreaHit(damage, AttackSyncDelay, areaRange: 4f, areaOffset: 2f));
            return;
        }

        // MATILDA (default)
        StartCoroutine(DelayedAreaHit(damage, AttackSyncDelay,
            areaRange: matildaScytheRange, areaOffset: matildaScytheOffset));
    }

    System.Collections.IEnumerator DelayedAreaHit(int damage, float delay, float areaRange, float areaOffset)
    {
        yield return new WaitForSeconds(delay);
        HitAreaSwing(damage, areaRange, areaOffset);
    }

    System.Collections.IEnumerator DelayedSwordHit(int damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        HitSwordSwing(damage, swordReach: 3f, swordWidth: 1.2f);
    }

    // Cerc de damage in fata personajului (Matilda + Vampire Girl)
    void HitAreaSwing(int damage, float areaRange, float areaOffset)
    {
        Vector2 dir    = player.FacingDirection;
        Vector2 center = (Vector2)transform.position + dir * areaOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, areaRange);
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }

    // Lovitura de sabie - capsula lunga pe directie (Dracula)
    void HitSwordSwing(int damage, float swordReach, float swordWidth)
    {
        Vector2 dir    = player.FacingDirection;
        Vector2 pStart = (Vector2)transform.position + dir * 0.5f;
        Vector2 pEnd   = pStart + dir * swordReach;
        Collider2D[] hits = Physics2D.OverlapCapsuleAll(
            (pStart + pEnd) * 0.5f,
            new Vector2(swordReach, swordWidth),
            Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical,
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }

    private static Sprite cachedBloodChargeSprite;

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        if (enemies == null || enemies.Length == 0) return null;

        Enemy target = null;
        float bestDist = float.MaxValue;
        Vector3 me = transform.position;
        foreach (var e in enemies)
        {
            if (e == null) continue;
            float d = (e.transform.position - me).sqrMagnitude;
            if (d < bestDist) { bestDist = d; target = e; }
        }
        return target;
    }

    // Countess: declanseaza animatia de atac, asteapta ca personajul sa termine swing-ul,
    // apoi spawn proiectilul. Asta sincronizeaza vizualul cu projectilul.
    System.Collections.IEnumerator CountessAttackSequence(Enemy target, int damage)
    {
        // 1. Trigger animatia INAINTE de spawn
        if (vampAnim == null) vampAnim = GetComponent<VampireAnimator>();
        if (vampAnim != null) vampAnim.TriggerAttack();

        // 2. Animatia Blood Charge porneste cu 0.4s inainte ca proiectilul sa iasa
        yield return new WaitForSeconds(0.4f);

        // 3. Verifica daca target-ul mai exista; daca nu, ia altul nou
        if (target == null) target = FindNearestEnemy();
        if (target == null) yield break;

        // 4. Spawn proiectile Blood Charge spre target. Numarul = stat Amount (Revolver).
        Vector3 dir = (target.transform.position - transform.position).normalized;
        if (bulletPrefab == null) yield break;

        int amount = Mathf.Max(1, PlayerStatsRuntime.GetIntStat(StatType.Amount, 1));
        for (int i = 0; i < amount; i++)
        {
            float offset = amount == 1 ? 0f : (i - (amount - 1) * 0.5f) * spreadAngle;
            Vector3 shotDir = Quaternion.Euler(0f, 0f, offset) * dir;

            GameObject bullet   = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.SetDirection(shotDir, areaScale: 1.4f);

            var sr = bullet.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (cachedBloodChargeSprite == null)
                    cachedBloodChargeSprite = LoadBloodChargeSprite();
                if (cachedBloodChargeSprite != null) sr.sprite = cachedBloodChargeSprite;
                else sr.color = new Color(0.9f, 0.1f, 0.2f, 1f);
            }
        }
    }

    static Sprite LoadBloodChargeSprite()
    {
        string path = System.IO.Path.Combine(Application.dataPath,
            "Sprites/Characters/Countess_Vampire/Blood_Charge_1.png");
        if (!System.IO.File.Exists(path)) return null;
        byte[] bytes = System.IO.File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        if (!tex.LoadImage(bytes)) return null;
        // Folosim primul cadru patrat (cellW = cellH)
        int cellH = tex.height;
        Rect r = new Rect(0, 0, cellH, cellH);
        return Sprite.Create(tex, r, new Vector2(0.5f, 0.5f), cellH / 1.2f, 0, SpriteMeshType.FullRect);
    }

    bool TryShoot()
    {
        // Hero ataca spre pozitia mouse-ului
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = (mousePos - transform.position).normalized;
        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.right;

        if (anim != null && anim.enabled)
            anim.SetTrigger("shoot");

        int   amount    = Mathf.Max(1, PlayerStatsRuntime.GetIntStat(StatType.Amount, 1));
        float areaScale = Mathf.Max(0.1f, PlayerStatsRuntime.GetMultiplier(StatType.Area));

        for (int i = 0; i < amount; i++)
        {
            float  offset       = amount == 1 ? 0f : (i - (amount - 1) * 0.5f) * spreadAngle;
            Vector3 shotDir     = Quaternion.Euler(0f, 0f, offset) * direction;
            GameObject bullet   = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
                bulletScript.SetDirection(shotDir, areaScale);
        }

        return true;
    }
}
