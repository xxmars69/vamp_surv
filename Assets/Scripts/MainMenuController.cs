using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject characterPanel;
    public GameObject creditsPanel;
    public string gameSceneName = "SampleScene";

    private CharacterSelectionData.CharacterType selectedCharacter = CharacterSelectionData.CharacterType.Hero;
    private Image heroCard;
    private Image matildaCard;
    private Image countessCard;
    private Image draculaCard;
    private Image vampireGirlCard;
    private GameObject statsPanel;

    // Paleta moderna - albastru petrol/portocaliu
    private readonly Color cardSelected   = new Color(0.95f, 0.55f, 0.20f, 1f);
    private readonly Color cardUnselected = new Color(0.15f, 0.22f, 0.30f, 1f);
    private readonly Color panelDark      = new Color(0.08f, 0.10f, 0.14f, 0.95f);
    private readonly Color buttonColor    = new Color(0.20f, 0.55f, 0.45f, 1f);

    private GameObject shopPanel;
    private readonly Image[] difficultyButtons = new Image[3];

    void Start()
    {
        SetPanel(mainPanel, true);
        SetPanel(characterPanel, false);
        SetPanel(creditsPanel, false);

        BuildCharacterSelectionUI();
        BuildShopUI();
    }

    // ── Main panel buttons ────────────────────────────────────────────────

    public void ShowCharacterSelection()
    {
        SetPanel(mainPanel, false);
        SetPanel(characterPanel, true);
        SetPanel(creditsPanel, false);
        RefreshSelectionState();
    }

    public void ShowCredits()
    {
        SetPanel(mainPanel, false);
        SetPanel(characterPanel, false);
        SetPanel(creditsPanel, true);
    }

    public void BackToMain()
    {
        SetPanel(mainPanel, true);
        SetPanel(characterPanel, false);
        SetPanel(creditsPanel, false);
    }

    public void StartGame()
    {
        CharacterSelectionData.Selected = selectedCharacter;
        SceneManager.LoadScene(gameSceneName);
    }

    // Butonul SETTINGS deschide acum magazinul de upgrade-uri permanente
    public void SettingsPlaceholder()
    {
        ShowShop();
    }

    public void ShowShop()
    {
        SetPanel(mainPanel, false);
        SetPanel(characterPanel, false);
        SetPanel(creditsPanel, false);
        SetPanel(shopPanel, true);
        RebuildShop();
    }

    void BackFromShop()
    {
        SetPanel(shopPanel, false);
        SetPanel(mainPanel, true);
    }

    // ── Magazin de power-up-uri permanente (Partile 26 + 28) ─────────────
    void BuildShopUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        shopPanel = CreateObject("ShopPanel", canvas.transform);
        RectTransform rect = shopPanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        shopPanel.AddComponent<Image>().color = panelDark;
        SetPanel(shopPanel, false);
    }

    void RebuildShop()
    {
        if (shopPanel == null) return;
        foreach (Transform c in shopPanel.transform) Destroy(c.gameObject);

        GameObject title = CreateLabel(shopPanel.transform, "MAGAZIN - UPGRADE-URI PERMANENTE", 30, FontStyle.Bold);
        AnchorTopCenter(title.GetComponent<RectTransform>(), 0f, -30f, 800f, 50f);

        GameObject goldLbl = CreateLabel(shopPanel.transform, "Gold total: " + MetaProgress.TotalGold, 24, FontStyle.Bold);
        AnchorTopCenter(goldLbl.GetComponent<RectTransform>(), 0f, -85f, 600f, 40f);
        goldLbl.GetComponent<Text>().color = new Color(1f, 0.9f, 0.3f);

        // Container vertical pentru randuri
        GameObject col = CreateObject("ShopCol", shopPanel.transform);
        RectTransform colRect = col.GetComponent<RectTransform>();
        colRect.anchorMin = new Vector2(0.5f, 0.5f);
        colRect.anchorMax = new Vector2(0.5f, 0.5f);
        colRect.pivot = new Vector2(0.5f, 0.5f);
        colRect.sizeDelta = new Vector2(520f, 240f);
        var vl = col.AddComponent<VerticalLayoutGroup>();
        vl.spacing = 14f; vl.childAlignment = TextAnchor.MiddleCenter;
        vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

        ShopRow(col.transform, "Damage  +1",
            MetaProgress.DamageLevel, MetaProgress.MaxDamage, MetaProgress.DamageCost,
            () => { if (MetaProgress.BuyDamage()) RebuildShop(); });
        ShopRow(col.transform, "Health  +1 inima",
            MetaProgress.HealthLevel, MetaProgress.MaxHealth, MetaProgress.HealthCost,
            () => { if (MetaProgress.BuyHealth()) RebuildShop(); });
        ShopRow(col.transform, "Speed  +1%",
            MetaProgress.SpeedLevel, MetaProgress.MaxSpeed, MetaProgress.SpeedCost,
            () => { if (MetaProgress.BuySpeed()) RebuildShop(); });

        GameObject back = CreateBigButton(shopPanel.transform, "BACK", new Color(0.30f, 0.30f, 0.32f, 1f), BackFromShop);
        RectTransform backRect = back.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0f, 0f); backRect.anchorMax = new Vector2(0f, 0f);
        backRect.pivot = new Vector2(0f, 0f);
        backRect.sizeDelta = new Vector2(110f, 38f);
        backRect.anchoredPosition = new Vector2(20f, 20f);
    }

    void ShopRow(Transform parent, string label, int level, int maxLevel, int cost, UnityEngine.Events.UnityAction onBuy)
    {
        bool maxed = level >= maxLevel;
        string text = label + "   (Lv " + level + "/" + maxLevel + ")   " + (maxed ? "MAX" : cost + " gold");
        GameObject btn = CreateBigButton(parent, text,
            maxed ? new Color(0.25f, 0.25f, 0.27f, 1f) : buttonColor, maxed ? (UnityEngine.Events.UnityAction)null : onBuy);
        LayoutElement le = btn.AddComponent<LayoutElement>();
        le.minHeight = 50f;
    }

    // ── Character selection UI ────────────────────────────────────────────

    void BuildCharacterSelectionUI()
    {
        if (characterPanel == null) return;

        foreach (Transform child in characterPanel.transform)
            Destroy(child.gameObject);

        // Fundalul panoului - intunecat, estompeaza scena de fundal
        Image bg = characterPanel.GetComponent<Image>();
        if (bg == null) bg = characterPanel.AddComponent<Image>();
        bg.color = panelDark;

        RemoveLayoutGroups(characterPanel);

        // Titlu in partea de sus
        GameObject title = CreateLabel(characterPanel.transform, "ALEGE PERSONAJUL", 34, FontStyle.Bold);
        AnchorTopCenter(title.GetComponent<RectTransform>(), 0f, -32f, 600f, 50f);

        // Panou STATS - in stanga, lipit de margine
        statsPanel = BuildStatsPanel(characterPanel.transform);

        // Container cu cele 2 carduri + buton START - in partea dreapta-centru
        GameObject contentRoot = CreateObject("ContentRoot", characterPanel.transform);
        RectTransform contentRect = contentRoot.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.28f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.offsetMin = new Vector2(20f, 30f);
        contentRect.offsetMax = new Vector2(-20f, -100f);

        // Carduri pe acelasi rand
        GameObject row = CreateObject("CharacterRow", contentRoot.transform);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 0.25f);
        rowRect.anchorMax = new Vector2(1f, 0.95f);
        rowRect.offsetMin = Vector2.zero;
        rowRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.spacing = 20f;
        rowLayout.childForceExpandWidth  = false;
        rowLayout.childForceExpandHeight = false;

        // Hero umple complet sprite-ul lui; Matilda are spatiu gol in jurul corpului in cadru.
        // Ca sa para la fel de mari vizual, micsoram portretul Hero (0.7) fata de Matilda (1.0).
        heroCard    = CreateCharacterCard(row.transform, "Hero",
                          RuntimeVisualRepair.LoadSpriteRuntime("Sprites/Hero/idle/idle01.png", 16f),
                          CharacterSelectionData.CharacterType.Hero, 0.7f);

        matildaCard = CreateCharacterCard(row.transform, "Matilda",
                          MatildaAnimator.LoadIconSprite(),
                          CharacterSelectionData.CharacterType.Matilda, 1f);

        countessCard = CreateCharacterCard(row.transform, "Countess",
                          VampireAnimator.LoadIcon("Countess_Vampire"),
                          CharacterSelectionData.CharacterType.Countess_Vampire, 1f);

        draculaCard = CreateCharacterCard(row.transform, "Dracula",
                          VampireAnimator.LoadIcon("Dracula"),
                          CharacterSelectionData.CharacterType.Dracula, 1f);

        vampireGirlCard = CreateCharacterCard(row.transform, "Vampire Girl",
                          VampireAnimator.LoadIcon("Vampire_Girl"),
                          CharacterSelectionData.CharacterType.Vampire_Girl, 1f);

        // Rand de dificultate (Usor / Normal / Greu) - adaptarea "Level Select"
        GameObject diffRow = CreateObject("DifficultyRow", contentRoot.transform);
        RectTransform drRect = diffRow.GetComponent<RectTransform>();
        drRect.anchorMin = new Vector2(0.5f, 0f);
        drRect.anchorMax = new Vector2(0.5f, 0f);
        drRect.pivot     = new Vector2(0.5f, 0f);
        drRect.sizeDelta = new Vector2(420f, 44f);
        drRect.anchoredPosition = new Vector2(0f, 100f);
        var drLayout = diffRow.AddComponent<HorizontalLayoutGroup>();
        drLayout.spacing = 10f; drLayout.childAlignment = TextAnchor.MiddleCenter;
        drLayout.childForceExpandWidth = true; drLayout.childForceExpandHeight = true;

        foreach (GameDifficulty.Level lvl in System.Enum.GetValues(typeof(GameDifficulty.Level)))
        {
            GameDifficulty.Level captured = lvl;
            GameObject db = CreateBigButton(diffRow.transform, GameDifficulty.Name(lvl),
                cardUnselected, () => { GameDifficulty.Selected = captured; RefreshSelectionState(); });
            difficultyButtons[(int)lvl] = db.GetComponent<Image>();
        }

        // Buton START - lat, jos
        GameObject startBtn = CreateBigButton(contentRoot.transform, "START", buttonColor, StartGame);
        RectTransform sbRect = startBtn.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0.5f, 0f);
        sbRect.anchorMax = new Vector2(0.5f, 0f);
        sbRect.pivot     = new Vector2(0.5f, 0f);
        sbRect.sizeDelta = new Vector2(260f, 60f);
        sbRect.anchoredPosition = new Vector2(0f, 30f);

        // Buton BACK in coltul stanga-jos
        GameObject backBtn = CreateBigButton(characterPanel.transform, "BACK", new Color(0.30f, 0.30f, 0.32f, 1f), BackToMain);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0f, 0f);
        backRect.anchorMax = new Vector2(0f, 0f);
        backRect.pivot     = new Vector2(0f, 0f);
        backRect.sizeDelta = new Vector2(110f, 38f);
        backRect.anchoredPosition = new Vector2(20f, 20f);

        RefreshSelectionState();
    }

    // ── Stats panel (live, depinde de selectedCharacter) ─────────────────

    GameObject BuildStatsPanel(Transform parent)
    {
        GameObject panel = CreateObject("StatsPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0.29f, 1f);
        rect.offsetMin = new Vector2(20f, 90f);
        rect.offsetMax = new Vector2(-10f, -100f);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);

        // Container vertical pentru continut
        GameObject content = CreateObject("Content", panel.transform);
        RectTransform cRect = content.GetComponent<RectTransform>();
        cRect.anchorMin = Vector2.zero;
        cRect.anchorMax = Vector2.one;
        cRect.offsetMin = new Vector2(16f, 16f);
        cRect.offsetMax = new Vector2(-16f, -16f);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.spacing = 6f;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;

        return panel;
    }

    void RebuildStatsPanel()
    {
        if (statsPanel == null) return;

        Transform content = statsPanel.transform.Find("Content");
        if (content == null) return;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        CharacterProfile.Profile profile = CharacterProfile.GetProfile(selectedCharacter);

        AddStatLine(content, "STATS - " + profile.name.ToUpper(), 26, FontStyle.Bold);
        AddStatLine(content, "", 8, FontStyle.Normal);
        AddStatLine(content, "Health: " + profile.hearts + " hp", 22, FontStyle.Bold);
        AddStatLine(content, "Speed: " + profile.speedPercent + "%", 22, FontStyle.Bold);
        AddStatLine(content, "Damage: " + profile.damage, 22, FontStyle.Bold);
        AddStatLine(content, "Attack: every " + profile.attackInterval.ToString("0.#") + "s", 20, FontStyle.Bold);
        AddStatLine(content, "Luck: " + profile.luckPercent + "%", 22, FontStyle.Bold);
        float hearts = profile.recoveryAmount * 0.5f;
        AddStatLine(content, "Recovery: " + hearts.ToString("0.#") + " / " + profile.recoveryInterval.ToString("0") + "s", 20, FontStyle.Bold);
        AddStatLine(content, "Shield: " + profile.shieldKills + " kills", 22, FontStyle.Bold);
    }

    void AddStatLine(Transform parent, string text, int fontSize, FontStyle style)
    {
        GameObject go = new GameObject("Stat", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);

        Text t = go.GetComponent<Text>();
        t.text      = text;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.color     = Color.white;
        t.alignment = TextAnchor.MiddleLeft;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight = fontSize + 8f;
    }

    // ── Character cards ───────────────────────────────────────────────────

    Image CreateCharacterCard(Transform parent, string charName, Sprite portraitSprite,
                              CharacterSelectionData.CharacterType type, float portraitScale = 1f)
    {
        GameObject card = CreateObject(charName + "Card", parent);
        LayoutElement le = card.AddComponent<LayoutElement>();
        le.minWidth  = 230f;
        le.minHeight = 300f;

        // Background-ul cardului devine si butonul de selectie
        Image cardImg = card.AddComponent<Image>();
        cardImg.color = cardUnselected;

        Button cardButton = card.AddComponent<Button>();
        ColorBlock cb = cardButton.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        cb.pressedColor     = new Color(0.85f, 0.85f, 0.85f, 1f);
        cardButton.colors   = cb;
        cardButton.targetGraphic = cardImg;

        CharacterSelectionData.CharacterType captured = type;
        cardButton.onClick.AddListener(() => SelectCharacter(captured));

        // Portret - centrat in partea de sus a cardului
        GameObject portrait = CreateObject("Portrait", card.transform);
        RectTransform pRect = portrait.GetComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.5f, 1f);
        pRect.anchorMax = new Vector2(0.5f, 1f);
        pRect.pivot     = new Vector2(0.5f, 1f);
        pRect.sizeDelta = new Vector2(200f * portraitScale, 230f * portraitScale);
        pRect.anchoredPosition = new Vector2(0f, -20f);

        Image portraitImg = portrait.AddComponent<Image>();
        portraitImg.preserveAspect = true;
        portraitImg.raycastTarget  = false;
        if (portraitSprite != null) portraitImg.sprite = portraitSprite;

        // Nume - jos pe card
        GameObject nameObject = CreateLabel(card.transform, charName, 22, FontStyle.Bold);
        RectTransform nameRect = nameObject.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 0f);
        nameRect.pivot     = new Vector2(0.5f, 0f);
        nameRect.sizeDelta = new Vector2(0f, 36f);
        nameRect.anchoredPosition = new Vector2(0f, 10f);
        nameObject.GetComponent<Text>().raycastTarget = false;

        return cardImg;
    }

    void SelectCharacter(CharacterSelectionData.CharacterType type)
    {
        selectedCharacter = type;
        RefreshSelectionState();
    }

    void RefreshSelectionState()
    {
        ColorCard(heroCard,        CharacterSelectionData.CharacterType.Hero);
        ColorCard(matildaCard,     CharacterSelectionData.CharacterType.Matilda);
        ColorCard(countessCard,    CharacterSelectionData.CharacterType.Countess_Vampire);
        ColorCard(draculaCard,     CharacterSelectionData.CharacterType.Dracula);
        ColorCard(vampireGirlCard, CharacterSelectionData.CharacterType.Vampire_Girl);

        // Evidentiaza dificultatea selectata
        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            if (difficultyButtons[i] == null) continue;
            difficultyButtons[i].color = (int)GameDifficulty.Selected == i ? cardSelected : cardUnselected;
        }

        RebuildStatsPanel();
    }

    void ColorCard(Image card, CharacterSelectionData.CharacterType type)
    {
        if (card == null) return;
        card.color = selectedCharacter == type ? cardSelected : cardUnselected;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    static void RemoveLayoutGroups(GameObject go)
    {
        VerticalLayoutGroup   v = go.GetComponent<VerticalLayoutGroup>();
        HorizontalLayoutGroup h = go.GetComponent<HorizontalLayoutGroup>();
        if (v != null) DestroyImmediate(v);
        if (h != null) DestroyImmediate(h);
    }

    static void AnchorTopCenter(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot     = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);
    }

    static GameObject CreateObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject CreateLabel(Transform parent, string text, int fontSize, FontStyle style = FontStyle.Normal)
    {
        GameObject go = new GameObject("Label_" + text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.text      = text;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.color     = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        return go;
    }

    static GameObject CreateBigButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label + "_Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        go.GetComponent<Button>().onClick.AddListener(action);

        GameObject labelObject = CreateLabel(go.transform, label, 22, FontStyle.Bold);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        labelObject.GetComponent<Text>().raycastTarget = false;

        return go;
    }
}
