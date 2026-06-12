using System.Collections.Generic;
using UnityEngine;
using CelikenVP;

// Arma "Book of Faith": carti care se rotesc mereu pe orbita jucatorului.
// Fiecare nivel adauga o carte (max 5). Cartile dau damage la atingere cu inamicii.
public class OrbitingBooks : MonoBehaviour
{
    public float orbitRadius = 1.6f;
    public float baseRotationSpeed = 120f; // grade/sec la nivel 1 (~viteza de baza a personajului)

    private readonly List<Transform> books = new();
    private float angle;
    private Sprite bookSprite;

    void Update()
    {
        // Rotatie continua a tuturor cartilor in jurul jucatorului
        angle += baseRotationSpeed * Time.deltaTime;
        Reposition();
    }

    // Evolutie: adauga carti peste limita normala + accelereaza rotatia
    public void Evolve(int extraBooks, float speedMultiplier)
    {
        baseRotationSpeed *= speedMultiplier;
        for (int i = 0; i < extraBooks; i++)
            AddBook(99); // ignora limita normala
    }

    // Apelat la fiecare pick al itemului - adauga o carte (pana la max)
    public void AddBook(int maxBooks)
    {
        if (books.Count >= maxBooks) return;

        if (bookSprite == null)
            bookSprite = RuntimeVisualRepair.LoadSpriteRuntime("Sprites/Items/Bible-1.png", 48f);

        GameObject book = new GameObject("Book_" + (books.Count + 1));
        book.transform.SetParent(transform, false);

        SpriteRenderer sr = book.AddComponent<SpriteRenderer>();
        sr.sprite = bookSprite;
        sr.sortingOrder = 40;

        CircleCollider2D col = book.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        book.AddComponent<BookDamage>();

        books.Add(book.transform);
        Reposition();
    }

    private void Reposition()
    {
        int count = books.Count;
        for (int i = 0; i < count; i++)
        {
            if (books[i] == null) continue;
            float a = (angle + i * (360f / count)) * Mathf.Deg2Rad;
            books[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * orbitRadius;
        }
    }
}

// Damage-ul cartii la contact cu inamicul
public class BookDamage : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) => Hit(other);
    void OnTriggerStay2D(Collider2D other)  => Hit(other);

    void Hit(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        float might = PlayerStatsRuntime.GetMultiplier(StatType.Might);
        int damage  = Mathf.Max(1, Mathf.RoundToInt(3 * might));
        enemy.TakeDamage(damage);
    }
}
