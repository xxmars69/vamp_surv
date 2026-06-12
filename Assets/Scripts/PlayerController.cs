using UnityEngine;
using System.Collections;
using CelikenVP;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float baseMoveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;
    private SpriteRenderer sr;

    public int maxHealth = 6;
    public int currentHealth;
    private HealthUI healthUI;

    // Directia in care se uita personajul (folosita de Matilda pentru atac)
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    public bool IsMatilda { get; private set; }
    // True pentru personajele care folosesc VampireAnimator (Countess, Dracula, VampireGirl)
    public bool IsVampire { get; private set; }
    private VampireAnimator vampAnim;

    // True cand jucatorul are input de miscare (folosit de MatildaAnimator)
    public bool IsMoving { get; private set; }

    // Directia de input (echivalentul lui PlayerMovement.moveDir din tutorial),
    // folosita de MapController pentru generarea de chunk-uri de teren
    public Vector2 moveDir => moveInput;

    // Recovery: valori venite din profilul personajului
    private float recoveryInterval = 15f;
    private int   recoveryAmount   = 1;
    private float recoveryTimer;

    // Damage multiplier specific personajului (Hero 1.2x, Matilda 0.8x)
    public float CharacterDamageMultiplier { get; private set; } = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Blocheaza rotatia jucatorului la coliziuni (ex. cu miniboss)
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.constraints    = RigidbodyConstraints2D.FreezeRotation;
        }

        // Aplicam profilul personajului ales (HP, recovery, damage, viteza)
        CharacterProfile.Profile profile = CharacterProfile.Current;
        maxHealth                 = profile.hearts * 2;
        currentHealth             = maxHealth;
        recoveryInterval          = profile.recoveryInterval;
        recoveryAmount            = Mathf.RoundToInt(profile.recoveryAmount);
        CharacterDamageMultiplier = profile.damage / 10f;
        baseMoveSpeed             = 5f * (profile.speedPercent / 5f);

        healthUI = Object.FindAnyObjectByType<HealthUI>();
        if (healthUI == null)
            healthUI = RuntimeVisualRepair.EnsureHealthUI();
        if (healthUI != null)
        {
            healthUI.EnsureHeartCount(profile.hearts);
            healthUI.UpdateHearts(currentHealth);
        }

        ApplyCharacterSelection();
    }

    void ApplyCharacterSelection()
    {
        var selected = CharacterSelectionData.Selected;
        IsMatilda = selected == CharacterSelectionData.CharacterType.Matilda;

        // Maparea personaj -> folder + fisier de atac (Vampire_Girl foloseste Attack_4)
        string vampireFolder = null;
        string attackFile = "Attack_1.png";
        switch (selected)
        {
            case CharacterSelectionData.CharacterType.Countess_Vampire:
                // Attack_1 arata personajul atacand; Blood_Charge e doar efectul/proiectilul
                // care e spawned separat ca glonte de catre AutoAttack
                vampireFolder = "Countess_Vampire"; attackFile = "Attack_1.png"; break;
            case CharacterSelectionData.CharacterType.Dracula:
                vampireFolder = "Dracula"; attackFile = "Attack_1.png"; break;
            case CharacterSelectionData.CharacterType.Vampire_Girl:
                vampireFolder = "Vampire_Girl"; attackFile = "Attack_4.png"; break;
        }
        IsVampire = vampireFolder != null;

        if (IsMatilda)
        {
            if (anim != null) anim.enabled = false;
            if (GetComponent<MatildaAnimator>() == null)
                gameObject.AddComponent<MatildaAnimator>();
        }
        else if (IsVampire)
        {
            if (anim != null) anim.enabled = false;
            vampAnim = GetComponent<VampireAnimator>();
            if (vampAnim == null) vampAnim = gameObject.AddComponent<VampireAnimator>();
            vampAnim.characterFolder = vampireFolder;
            vampAnim.attackFile = attackFile;
        }
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        IsMoving = moveInput.magnitude > 0;

        if (anim != null && anim.enabled) anim.SetBool("isMoving", IsMoving);

        TickRecovery();
        TickBuffs();

        if (IsMatilda || IsVampire)
        {
            // Matilda + vampirele: se uita in directia de miscare
            if (moveInput.sqrMagnitude > 0.01f)
            {
                FacingDirection = moveInput.normalized;
                if (sr != null) sr.flipX = moveInput.x < 0f;
            }
        }
        else
        {
            // Hero: se uita spre mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            sr.flipX = mousePos.x < transform.position.x;
            FacingDirection = sr.flipX ? Vector2.left : Vector2.right;
        }
    }

    void FixedUpdate()
    {
        if (currentHealth <= 0) return;
        float statSpeed = baseMoveSpeed * PlayerStatsRuntime.GetMultiplier(StatType.MoveSpeed) * speedBuffMult;
        rb.MovePosition(rb.position + moveInput.normalized * statSpeed * Time.fixedDeltaTime);
    }

    // ── Buff/Debuff temporar (Partea 23) ────────────────────────────────
    private float speedBuffMult = 1f;
    private float speedBuffTimer;

    public void ApplySpeedBuff(float multiplier, float duration)
    {
        speedBuffMult = multiplier;
        speedBuffTimer = duration;
    }

    void TickBuffs()
    {
        if (speedBuffTimer > 0f)
        {
            speedBuffTimer -= Time.deltaTime;
            if (speedBuffTimer <= 0f) speedBuffMult = 1f;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        // Scutul (itemul Shield) anuleaza complet lovitura daca e activ
        ShieldSystem shield = GetComponent<ShieldSystem>();
        if (shield != null && shield.TryAbsorbHit()) return;

        int armor = PlayerStatsRuntime.GetIntStat(StatType.Armor, 0);
        int finalDamage = Mathf.Max(1, damage - armor);
        currentHealth -= finalDamage;
        StartCoroutine(FlashRed());
        SoundManager.Play(SoundManager.Sfx.Hurt);

        if (healthUI != null) healthUI.UpdateHearts(currentHealth);

        if (anim != null) { try { anim.SetTrigger("damage"); } catch {} }

        if (currentHealth <= 0) Die();
    }

    // Recovery pasiv: vindeca 0.5 inima la fiecare 15 secunde, pana la maxim
    void TickRecovery()
    {
        if (currentHealth >= maxHealth) { recoveryTimer = 0f; return; }

        recoveryTimer += Time.deltaTime;
        if (recoveryTimer >= recoveryInterval)
        {
            recoveryTimer -= recoveryInterval;
            currentHealth = Mathf.Min(currentHealth + recoveryAmount, maxHealth);
            if (healthUI != null) healthUI.UpdateHearts(currentHealth);
        }
    }

    // Itemul "Ham": vindeca un procent din viata maxima (ex. 25%)
    public void HealPercent(float percent)
    {
        if (currentHealth <= 0) return;
        int heal = Mathf.Max(1, Mathf.RoundToInt(maxHealth * percent));
        currentHealth = Mathf.Min(currentHealth + heal, maxHealth);
        if (healthUI == null) healthUI = Object.FindAnyObjectByType<HealthUI>();
        if (healthUI != null) healthUI.UpdateHearts(currentHealth);
    }

    // Itemul "Heart": reduce intervalul de regen si creste cantitatea de heal
    public void BoostRecovery(float intervalReduce, int amountAdd)
    {
        recoveryInterval = Mathf.Max(2f, recoveryInterval - intervalReduce);
        recoveryAmount   = Mathf.Min(recoveryAmount + amountAdd, 6);
    }

    public float RecoveryInterval => recoveryInterval;
    public int   RecoveryAmount   => recoveryAmount;

    // Itemul "Beer": adauga o inima noua (goala) la maxim si vindeca o inima (+2 HP intern).
    public void AddHeart()
    {
        maxHealth += 2;
        currentHealth = Mathf.Min(currentHealth + 2, maxHealth);

        if (healthUI == null) healthUI = Object.FindAnyObjectByType<HealthUI>();
        if (healthUI != null)
        {
            healthUI.EnsureHeartCount(maxHealth / 2);
            healthUI.UpdateHearts(currentHealth);
        }
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    void Die()
    {
        Debug.Log("Jucatorul a murit!");
        if (anim != null && anim.enabled) { try { anim.SetTrigger("dead"); } catch {} }
        // Animatia de moarte pentru vampire (Countess/Dracula/Vampire Girl)
        if (vampAnim != null) vampAnim.TriggerDead();
        rb.linearVelocity = Vector2.zero;
    }
}
