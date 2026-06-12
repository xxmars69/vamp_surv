using System.IO;
using UnityEngine;

// Animator runtime pentru Matilda: taie sprite sheet-ul (576x256, grila 9x4, celula 64x64)
// in cadre individuale si le ruleaza cadru-cu-cadru (idle / alergare / atac).
public class MatildaAnimator : MonoBehaviour
{
    public const int CELL = 64;
    // PPU ales astfel incat o celula de 64px sa aiba 2.5 units inaltime - exact ca Hero (40px @ 16 PPU).
    public const float PPU = 25.6f;

    public float fps = 10f;

    private SpriteRenderer sr;
    private PlayerController player;

    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite[] attackFrames;

    private enum State { Idle, Run, Attack }
    private State state = State.Idle;
    private Sprite[] current;
    private int frameIndex;
    private float timer;
    private bool attacking;

    private static Texture2D cachedTexture;

    void Awake()
    {
        sr     = GetComponent<SpriteRenderer>();
        player = GetComponent<PlayerController>();

        Texture2D tex = LoadSheetTexture();
        if (tex != null)
        {
            idleFrames   = Slice(tex, rowFromTop: 0, count: 4);
            runFrames    = Slice(tex, rowFromTop: 1, count: 9);
            attackFrames = Slice(tex, rowFromTop: 3, count: 8);
        }

        current = idleFrames;
        if (current != null && current.Length > 0 && sr != null)
            sr.sprite = current[0];
    }

    void Update()
    {
        if (current == null) return;

        State desired;
        if (attacking)                                   desired = State.Attack;
        else if (player != null && player.IsMoving)      desired = State.Run;
        else                                             desired = State.Idle;

        if (desired != state)
        {
            state      = desired;
            frameIndex = 0;
            timer      = 0f;
            current    = PickFrames(state);
            SetCurrentSprite();
        }

        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer -= 1f / fps;
            frameIndex++;

            if (frameIndex >= current.Length)
            {
                if (state == State.Attack)
                    attacking = false; // atacul s-a terminat -> revine la idle/run
                frameIndex = 0;
            }

            SetCurrentSprite();
        }
    }

    // Apelat de AutoAttack cand Matilda trage.
    public void TriggerAttack()
    {
        if (attackFrames == null || attackFrames.Length == 0) return;
        attacking  = true;
        if (state != State.Attack)
        {
            state      = State.Attack;
            frameIndex = 0;
            timer      = 0f;
            current    = attackFrames;
            SetCurrentSprite();
        }
    }

    private Sprite[] PickFrames(State s)
    {
        switch (s)
        {
            case State.Attack: return attackFrames;
            case State.Run:    return runFrames;
            default:           return idleFrames;
        }
    }

    private void SetCurrentSprite()
    {
        if (sr == null || current == null || current.Length == 0) return;
        sr.sprite = current[Mathf.Clamp(frameIndex, 0, current.Length - 1)];
    }

    private static Sprite[] Slice(Texture2D tex, int rowFromTop, int count)
    {
        Sprite[] arr = new Sprite[count];
        int y = tex.height - CELL * (rowFromTop + 1); // origine textura = jos-stanga
        for (int i = 0; i < count; i++)
        {
            Rect r = new Rect(i * CELL, y, CELL, CELL);
            arr[i] = Sprite.Create(tex, r, new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect);
        }
        return arr;
    }

    // Un singur cadru (idle, stanga-sus) pentru iconita din meniu.
    public static Sprite LoadIconSprite()
    {
        Texture2D tex = LoadSheetTexture();
        if (tex == null) return null;
        int y = tex.height - CELL;
        Rect r = new Rect(0, y, CELL, CELL);
        return Sprite.Create(tex, r, new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect);
    }

    private static Texture2D LoadSheetTexture()
    {
        if (cachedTexture != null) return cachedTexture;

        string path = Path.Combine(Application.dataPath, "Sprites/Matilda/Matilda_Sheet.png");
        if (!File.Exists(path)) return null;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        if (!tex.LoadImage(bytes)) return null;

        cachedTexture = tex;
        return tex;
    }
}
