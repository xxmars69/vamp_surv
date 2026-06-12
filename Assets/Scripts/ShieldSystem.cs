using UnityEngine;

// Itemul "Shield": la fiecare N kill-uri genereaza un scut vizual in jurul jucatorului
// care anuleaza complet urmatoarea lovitura primita.
// N porneste de la 5 si scade cu 1 la fiecare nivel al itemului (nivel 5 => 1 kill).
public class ShieldSystem : MonoBehaviour
{
    public static ShieldSystem Instance { get; private set; }

    private int itemLevel;
    private int killsSinceShield;
    private bool shieldActive;
    private GameObject shieldVisual;

    public bool HasItem => itemLevel > 0;
    public bool ShieldActive => shieldActive;
    public int KillsRequired => Mathf.Max(1, 6 - Mathf.Clamp(itemLevel, 1, 5));

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetItemLevel(int level)
    {
        itemLevel = Mathf.Max(itemLevel, level);
    }

    // Apelat de Enemy.Die() prin metoda statica, ca sa nu depinda de referinte de scena.
    public static void RegisterKill()
    {
        if (Instance != null) Instance.OnKill();
    }

    private void OnKill()
    {
        if (itemLevel <= 0) return;

        // Cat timp scutul e activ, contorul de kill-uri pentru urmatorul scut sta pe loc
        if (shieldActive) return;

        killsSinceShield++;
        if (killsSinceShield >= KillsRequired)
        {
            killsSinceShield = 0;
            ActivateShield();
        }
    }

    // PlayerController intreaba scutul inainte sa aplice damage; true = lovitura anulata.
    public bool TryAbsorbHit()
    {
        if (!shieldActive) return false;

        shieldActive = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        return true;
    }

    private void ActivateShield()
    {
        shieldActive = true;
        if (shieldVisual == null)
            shieldVisual = CreateShieldVisual();
        shieldVisual.SetActive(true);
    }

    private GameObject CreateShieldVisual()
    {
        GameObject go = new GameObject("ShieldVisual");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRingSprite();
        sr.sortingOrder = 50;
        return go;
    }

    private static Sprite CreateRingSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color ring  = new Color(0.3f, 0.85f, 1f, 0.9f);
        Color fill  = new Color(0.3f, 0.85f, 1f, 0.12f);
        Color clear = new Color(0f, 0f, 0f, 0f);

        float center = (size - 1) / 2f;
        float outer  = size / 2f - 2f;
        float inner  = outer - 6f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (d <= outer && d >= inner)      tex.SetPixel(x, y, ring);
                else if (d < inner)                tex.SetPixel(x, y, fill);
                else                               tex.SetPixel(x, y, clear);
            }
        }
        tex.Apply();

        // 128 px la PPU 38 => ~3.4 units diametru, inconjoara jucatorul de 2.5 units
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 38f, 0, SpriteMeshType.FullRect);
    }
}
