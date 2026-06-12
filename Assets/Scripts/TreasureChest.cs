using System.IO;
using UnityEngine;

// Cufar dropat de miniboss. La contact cu jucatorul ofera un upgrade (level-up choice).
public class TreasureChest : MonoBehaviour
{
    private static Sprite chestSprite;
    private bool opened;

    public static void Spawn(Vector3 pos)
    {
        GameObject go = new GameObject("TreasureChest");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite();
        sr.sortingOrder = 4;
        go.transform.localScale = Vector3.one * 1.3f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        go.AddComponent<TreasureChest>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (opened || !col.CompareTag("Player")) return;
        opened = true;
        SoundManager.Play(SoundManager.Sfx.Chest);
        if (GameManager.Instance != null) GameManager.Instance.OpenUpgradeChoice();
        Destroy(gameObject);
    }

    static Sprite GetSprite()
    {
        if (chestSprite != null) return chestSprite;
        // Reutilizam imaginea de MoneyBag ca recompensa
        string path = Path.Combine(Application.dataPath, "CurrencyGems/MoneyBag/MoneyBag.png");
        if (!File.Exists(path)) return null;
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        if (!tex.LoadImage(bytes)) return null;
        chestSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), tex.height / 1f, 0, SpriteMeshType.FullRect);
        return chestSprite;
    }
}
