using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthUI : MonoBehaviour
{
    [Header("Hearts")]
    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    [Header("Stats")]
    public Text killText;
    public Text timerText;
    public GameObject gameOverPanel;

    private int killCount = 0;
    private float startTime;
    private bool isDead = false;

    // Dimensiuni HUD - modifica aici daca vrei mai mare/mai mic
    private const float HeartSize    = 16f;
    private const float HeartSpacing = 19f;

    void Start()
    {
        startTime = Time.time;
        NormalizeHudSizes();
    }

    // HUD compact in coltul stanga-sus: LV/XP (randul 1), inimi (randul 2), Gold (randul 3).
    // Kill counter marunt in dreapta-sus. Pozitiile sunt fortate din cod,
    // indiferent cum sunt salvate in scena.
    void NormalizeHudSizes()
    {
        if (heartImages != null && heartImages.Length > 0 && heartImages[0] != null)
        {
            // Mutam containerul-parinte al inimilor fix sub randul LV/XP (coltul stanga-sus)
            RectTransform parent = heartImages[0].rectTransform.parent as RectTransform;
            if (parent != null)
            {
                parent.anchorMin = new Vector2(0f, 1f);
                parent.anchorMax = new Vector2(0f, 1f);
                parent.pivot     = new Vector2(0f, 1f);
                parent.anchoredPosition = new Vector2(12f, -30f); // sub LV/XP (care e la -8)
            }

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] == null) continue;
                RectTransform rect = heartImages[i].rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot     = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(HeartSize, HeartSize);
                rect.anchoredPosition = new Vector2(i * HeartSpacing, 0f); // local fata de container
            }
        }

        // Timer sus de tot, centrat
        ShrinkText(timerText, 22);
        if (timerText != null)
            timerText.rectTransform.anchoredPosition = new Vector2(0f, -4f);

        EnsureKillText();
        if (killText != null)
        {
            ShrinkText(killText, 14);
            RectTransform rect = killText.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot     = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(220f, 22f);
            rect.anchoredPosition = new Vector2(-12f, -8f);
            killText.alignment = TextAnchor.MiddleRight;
            killText.text = "Kills: 0";
        }
    }

    // Creeaza textul de kills daca nu exista in scena
    void EnsureKillText()
    {
        if (killText != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject textObject = new GameObject("KillText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(canvas.transform, false);

        killText = textObject.GetComponent<Text>();
        killText.font  = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        killText.color = Color.white;
    }

    public static void ShrinkText(Text text, int fontSize)
    {
        if (text == null) return;
        text.resizeTextForBestFit = false;
        text.fontSize = fontSize;
    }

    // Pozitioneaza un text HUD in coltul stanga-sus, la pozitia data
    public static void PositionTopLeft(Text text, Vector2 position, int fontSize)
    {
        if (text == null) return;
        ShrinkText(text, fontSize);

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot     = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(320f, 22f);
        rect.anchoredPosition = position;
        text.alignment = TextAnchor.MiddleLeft;
    }

    void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // Actualizam Timer-ul
        float t = Time.time - startTime;
        string minutes = ((int)t / 60).ToString("00");
        string seconds = (t % 60).ToString("00");
        if (timerText != null) timerText.text = minutes + ":" + seconds;
    }

    // Extinde randul de inimi cand creste viata maxima (ex. itemul Beer adauga a 4-a inima).
    public void EnsureHeartCount(int hearts)
    {
        if (heartImages == null || heartImages.Length == 0 || heartImages.Length >= hearts)
            return;

        var list = new System.Collections.Generic.List<Image>(heartImages);
        Image template = list[list.Count - 1];

        // Pastram distanta dintre ultimele doua inimi existente
        Vector2 spacing = new Vector2(42f, 0f);
        if (list.Count >= 2)
        {
            Vector2 delta = list[list.Count - 1].rectTransform.anchoredPosition
                          - list[list.Count - 2].rectTransform.anchoredPosition;
            if (delta.sqrMagnitude > 1f) spacing = delta;
        }

        while (list.Count < hearts)
        {
            Image clone = Instantiate(template, template.transform.parent);
            clone.name = "Heart_" + (list.Count + 1);
            clone.rectTransform.anchoredPosition =
                list[list.Count - 1].rectTransform.anchoredPosition + spacing;
            clone.sprite = emptyHeart;
            list.Add(clone);
        }

        heartImages = list.ToArray();
    }

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            int heartValue = (i + 1) * 2;
            if (currentHealth >= heartValue) heartImages[i].sprite = fullHeart;
            else if (currentHealth == heartValue - 1) heartImages[i].sprite = halfHeart;
            else heartImages[i].sprite = emptyHeart;
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            // Ecran de rezultate
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
            SoundManager.Play(SoundManager.Sfx.GameOver);
        }
    }

    public void AddKill()
    {
        killCount++;
        if (killText != null) killText.text = "Kills: " + killCount;
    }

    public int   KillCount    => killCount;
    public float SurvivalTime => Time.time - startTime;
}
