using System.IO;
using UnityEngine;

// Animator runtime pentru personajele vampire (Countess, Dracula, Vampire_Girl).
// Sheet-uri strip orizontal de cadre 128x128. Suporta Idle/Run/Attack/Dead.
public class VampireAnimator : MonoBehaviour
{
    public const int CELL = 128;
    public const float PPU = 51f; // 128/51 ≈ 2.5 units, identic cu Hero si Matilda

    public float fps = 10f;
    public string characterFolder;
    public string attackFile = "Attack_1.png"; // per-personaj (ex. "Attack_4.png" pt Vampire_Girl)

    private SpriteRenderer sr;
    private PlayerController player;

    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite[] attackFrames;
    private Sprite[] deadFrames;

    private enum State { Idle, Run, Attack, Dead }
    private State state = State.Idle;
    private Sprite[] current;
    private int frameIndex;
    private float timer;
    private bool attacking;
    private bool dying;

    void Awake()
    {
        sr     = GetComponent<SpriteRenderer>();
        player = GetComponent<PlayerController>();
    }

    void Start()
    {
        if (string.IsNullOrEmpty(characterFolder)) return;

        idleFrames   = LoadStrip("Idle.png");
        runFrames    = LoadStrip("Run.png");
        attackFrames = LoadStrip(attackFile);
        deadFrames   = LoadStrip("Dead.png");

        current = idleFrames;
        if (current != null && current.Length > 0 && sr != null)
            sr.sprite = current[0];
    }

    void Update()
    {
        if (current == null) return;

        // Daca jucatorul a murit, ramane pe ultimul cadru de Dead
        if (dying)
        {
            AdvanceFrame(loop: false);
            return;
        }

        State desired;
        if (attacking)                              desired = State.Attack;
        else if (player != null && player.IsMoving) desired = State.Run;
        else                                        desired = State.Idle;

        if (desired != state)
        {
            state      = desired;
            frameIndex = 0;
            timer      = 0f;
            current    = Pick(state);
            SetCurrentSprite();
        }

        AdvanceFrame(loop: true);
    }

    public void TriggerAttack()
    {
        if (dying || attackFrames == null || attackFrames.Length == 0) return;
        attacking = true;
        if (state != State.Attack)
        {
            state      = State.Attack;
            frameIndex = 0;
            timer      = 0f;
            current    = attackFrames;
            SetCurrentSprite();
        }
    }

    // Apelat de PlayerController cand jucatorul moare
    public void TriggerDead()
    {
        if (deadFrames == null || deadFrames.Length == 0) return;
        dying      = true;
        state      = State.Dead;
        frameIndex = 0;
        timer      = 0f;
        current    = deadFrames;
        SetCurrentSprite();
    }

    private void AdvanceFrame(bool loop)
    {
        timer += Time.deltaTime;
        if (timer < 1f / fps) return;
        timer -= 1f / fps;
        frameIndex++;
        if (frameIndex >= current.Length)
        {
            if (state == State.Attack) attacking = false;
            if (!loop) { frameIndex = current.Length - 1; return; }
            frameIndex = 0;
        }
        SetCurrentSprite();
    }

    private Sprite[] Pick(State s)
    {
        switch (s)
        {
            case State.Attack: return attackFrames;
            case State.Run:    return runFrames;
            case State.Dead:   return deadFrames;
            default:           return idleFrames;
        }
    }

    private void SetCurrentSprite()
    {
        if (sr == null || current == null || current.Length == 0) return;
        sr.sprite = current[Mathf.Clamp(frameIndex, 0, current.Length - 1)];
    }

    private Sprite[] LoadStrip(string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Sprites/Characters", characterFolder, fileName);
        if (!File.Exists(path)) return new Sprite[0];
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        if (!tex.LoadImage(bytes)) return new Sprite[0];

        // Detectam dimensiunea cadrului dinamic (cadre patrate aliniate orizontal):
        // Idle/Run/Attack au cadre 128x128, dar Blood_Charge are 48x48 sau alt format
        int cellH = tex.height;
        int cellW = cellH;
        int count = Mathf.Max(1, tex.width / cellW);
        // Ajustam PPU astfel incat fiecare cadru sa fie ~2.5 units, indiferent de marime
        float ppu = cellH / 2.5f;

        Sprite[] frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            int x = i * cellW;
            if (x + cellW > tex.width) break;
            Rect r = new Rect(x, 0, cellW, cellH);
            frames[i] = Sprite.Create(tex, r, new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
        }
        return frames;
    }

    public static Sprite LoadIcon(string characterFolder)
    {
        string path = Path.Combine(Application.dataPath, "Sprites/Characters", characterFolder, "Idle.png");
        if (!File.Exists(path)) return null;
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        if (!tex.LoadImage(bytes)) return null;
        Rect r = new Rect(0, 0, CELL, CELL);
        return Sprite.Create(tex, r, new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect);
    }
}
