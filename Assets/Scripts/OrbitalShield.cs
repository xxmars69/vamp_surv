using UnityEngine;

public class OrbitalShield : MonoBehaviour
{
    public float rotationSpeed = 150f;
    public float damageRadius = 1.5f;

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            Destroy(col.gameObject);
        }
    }
}
