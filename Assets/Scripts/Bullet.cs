using UnityEngine;
using CelikenVP;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    [SerializeField] private int baseDamage = 1;
    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    public void SetDirection(Vector3 dir)
    {
        SetDirection(dir, 1f);
    }

    public void SetDirection(Vector3 dir, float areaScale)
    {
        direction = dir;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.localScale = transform.localScale * areaScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * PlayerStatsRuntime.GetMultiplier(StatType.Might)));
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);
            else
                Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
