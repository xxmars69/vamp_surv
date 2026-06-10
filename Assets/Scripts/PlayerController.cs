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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        maxHealth = Mathf.Max(maxHealth, 6);
        currentHealth = maxHealth;

        healthUI = Object.FindAnyObjectByType<HealthUI>();
        if (healthUI == null)
            healthUI = RuntimeVisualRepair.EnsureHealthUI();
        if (healthUI != null) healthUI.UpdateHearts(currentHealth);

        ApplyCharacterSelection();
    }

    void ApplyCharacterSelection()
    {
        IsMatilda = CharacterSelectionData.Selected == CharacterSelectionData.CharacterType.Matilda;

        if (!IsMatilda) return;

        // Dezactivam animator-ul Hero pentru Matilda (nu are acelasi controller)
        if (anim != null) anim.enabled = false;

        // Incarcam sprite-ul Matildei
        Sprite matildaSprite = RuntimeVisualRepair.LoadSpriteRuntime("Sprites/Matilda/Matilda_Sheet.png", 1f);
        if (matildaSprite != null && sr != null)
            sr.sprite = matildaSprite;
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (anim != null) anim.SetBool("isMoving", moveInput.magnitude > 0);

        if (IsMatilda)
        {
            // Matilda: se uita in directia de miscare
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
        float statSpeed = baseMoveSpeed * PlayerStatsRuntime.GetMultiplier(StatType.MoveSpeed);
        rb.MovePosition(rb.position + moveInput.normalized * statSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        int armor = PlayerStatsRuntime.GetIntStat(StatType.Armor, 0);
        int finalDamage = Mathf.Max(1, damage - armor);
        currentHealth -= finalDamage;
        StartCoroutine(FlashRed());

        if (healthUI != null) healthUI.UpdateHearts(currentHealth);

        if (anim != null) { try { anim.SetTrigger("damage"); } catch {} }

        if (currentHealth <= 0) Die();
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
        if (anim != null) { try { anim.SetTrigger("dead"); } catch {} }
        rb.linearVelocity = Vector2.zero;
    }
}
