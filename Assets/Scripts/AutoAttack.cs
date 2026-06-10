using UnityEngine;
using CelikenVP;

public class AutoAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    [SerializeField] private float baseFireInterval = 1f;
    [SerializeField] private float spreadAngle = 12f;
    private float nextFireTime;
    private Animator anim;
    private PlayerController player;

    void Start()
    {
        anim   = GetComponent<Animator>();
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (player == null || player.currentHealth <= 0) return;

        float cooldownPercent = PlayerStatsRuntime.GetPercentStat(StatType.Cooldown, 0f);
        float fireInterval    = baseFireInterval * Mathf.Clamp01(1f - cooldownPercent / 100f);
        fireInterval          = Mathf.Max(0.08f, fireInterval);

        if (Time.time >= nextFireTime)
        {
            if (TryShoot())
                nextFireTime = Time.time + fireInterval;
        }
    }

    bool TryShoot()
    {
        Vector3 direction;

        if (player != null && player.IsMatilda)
        {
            // Matilda ataca doar in directia in care priveste/se misca
            direction = player.FacingDirection;
        }
        else
        {
            // Hero ataca spre pozitia mouse-ului
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            direction  = (mousePos - transform.position).normalized;
            if (direction.sqrMagnitude <= 0.001f)
                direction = transform.right;
        }

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
