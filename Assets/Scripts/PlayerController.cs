using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;
    private SpriteRenderer sr;

    public int maxHealth = 6;
    public int currentHealth;
    private HealthUI healthUI;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        
        // Cautam UI-ul si il actualizam instant
        healthUI = Object.FindFirstObjectByType<HealthUI>();
        if (healthUI != null) healthUI.UpdateHearts(currentHealth);
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (anim != null) anim.SetBool("isMoving", moveInput.magnitude > 0);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x < transform.position.x)
            sr.flipX = true;
        else
            sr.flipX = false;
    }

    void FixedUpdate()
    {
        if (currentHealth <= 0) return;
        rb.MovePosition(rb.position + moveInput.normalized * speed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
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
        // Ecranul de Game Over este activat automat de HealthUI
    }
}
