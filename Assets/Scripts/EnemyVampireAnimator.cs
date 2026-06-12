using System.IO;
using UnityEngine;

// Animeaza un inamic de tip Vampires2 (sprite 4-direction, cadre 64x64).
// Se ataseaza pe acelasi GameObject ca Enemy.cs si conduce SpriteRenderer-ul.
public class EnemyVampireAnimator : MonoBehaviour
{
    public const int CELL = 64;
    public string folder = "Enemies2";
    public float fps = 8f;

    private SpriteRenderer sr;
    private Sprite[] walkFrames;
    private int frameIndex;
    private float timer;

    private static readonly System.Collections.Generic.Dictionary<string, Texture2D> texCache = new();

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        walkFrames = LoadStrip("Walk.png");
        if (walkFrames.Length > 0) sr.sprite = walkFrames[0];
    }

    void Update()
    {
        if (walkFrames == null || walkFrames.Length == 0) return;
        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer -= 1f / fps;
            frameIndex = (frameIndex + 1) % walkFrames.Length;
            sr.sprite = walkFrames[frameIndex];
        }
    }

    // 4-direction: folosim primul rand (jos = directia spre camera)
    Sprite[] LoadStrip(string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Sprites", folder, fileName);
        if (!File.Exists(path)) return new Sprite[0];

        if (!texCache.TryGetValue(path, out var tex))
        {
            byte[] bytes = File.ReadAllBytes(path);
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            if (!tex.LoadImage(bytes)) return new Sprite[0];
            texCache[path] = tex;
        }

        int cols = tex.width / CELL;
        int rowY = tex.height - CELL; // randul de sus al texturii (down-facing pentru aceste pack-uri)
        Sprite[] arr = new Sprite[cols];
        // PPU 40 => cadrul 64px ≈ 1.6 units, mai mare ca liliacul
        for (int i = 0; i < cols; i++)
        {
            Rect r = new Rect(i * CELL, rowY, CELL, CELL);
            arr[i] = Sprite.Create(tex, r, new Vector2(0.5f, 0.5f), 40f, 0, SpriteMeshType.FullRect);
        }
        return arr;
    }
}
