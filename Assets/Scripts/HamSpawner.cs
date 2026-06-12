using System.IO;
using UnityEngine;

// Spawneaza itemul "Ham" la fiecare 30s in zona jucatorului. La ridicare vindeca 25% HP.
public class HamSpawner : MonoBehaviour
{
    public Transform player;
    public float interval = 30f;
    public float spawnRadius = 5f;
    public float healPercent = 0.25f;

    private float timer;
    private static Sprite hamSprite;

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            SpawnHam();
        }
    }

    void SpawnHam()
    {
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = player.position + new Vector3(offset.x, offset.y, 0f);

        GameObject ham = new GameObject("Ham");
        ham.transform.position = pos;

        var sr = ham.AddComponent<SpriteRenderer>();
        sr.sprite = GetHamSprite();
        sr.sortingOrder = 3;

        var col = ham.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var pickup = ham.AddComponent<HamPickup>();
        pickup.healPercent = healPercent;
    }

    static Sprite GetHamSprite()
    {
        if (hamSprite != null) return hamSprite;
        string path = Path.Combine(Application.dataPath, "Sprites/Items/Ham-1.png");
        if (!File.Exists(path)) return null;
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        if (!tex.LoadImage(bytes)) return null;
        // PPU astfel incat sa fie cam de marimea unui liliac (~0.9 units)
        float ppu = tex.height / 0.9f;
        hamSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
        return hamSprite;
    }
}

// Ridicarea de Ham: vindeca jucatorul la contact
public class HamPickup : MonoBehaviour
{
    public float healPercent = 0.25f;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        var pc = col.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.HealPercent(healPercent);
            pc.ApplySpeedBuff(1.5f, 6f); // buff: +50% viteza 6 secunde
        }
        SoundManager.Play(SoundManager.Sfx.Pickup);
        Destroy(gameObject);
    }
}
