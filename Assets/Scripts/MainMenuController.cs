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
    private Button startGameButton;
    private Image heroCard;
    private Image matildaCard;

    private readonly Color selectedColor   = new Color(0.2f, 0.7f, 0.3f, 1f);
    private readonly Color unselectedColor = new Color(0.15f, 0.22f, 0.30f, 1f);

    void Start()
    {
        SetPanel(mainPanel, true);
        SetPanel(characterPanel, false);
        SetPanel(creditsPanel, false);

        BuildCharacterSelectionUI();
    }

    // ── Main panel buttons ────────────────────────────────────────────────

    public void ShowCharacterSelection()
    {
        SetPanel(mainPanel, false);
        SetPanel(characterPanel, true);
        SetPanel(creditsPanel, false);
        RefreshCardColors();
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

    // ── Character selection UI built in code ─────────────────────────────

    void BuildCharacterSelectionUI()
    {
        if (characterPanel == null) return;

        foreach (Transform child in characterPanel.transform)
            Destroy(child.gameObject);

        // Layout group on the panel
        VerticalLayoutGroup vLayout = characterPanel.GetComponent<VerticalLayoutGroup>();
        if (vLayout == null) vLayout = characterPanel.AddComponent<VerticalLayoutGroup>();
        vLayout.childAlignment = TextAnchor.MiddleCenter;
        vLayout.spacing = 20f;
        vLayout.padding = new RectOffset(30, 30, 30, 30);
        vLayout.childForceExpandWidth  = false;
        vLayout.childForceExpandHeight = false;

        CreateLabel(characterPanel.transform, "Alege personajul", 30);

        // Row with the two character cards
        GameObject row = CreateObject("CharacterRow", characterPanel.transform);
        HorizontalLayoutGroup hLayout = row.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.spacing = 40f;
        hLayout.childForceExpandWidth  = false;
        hLayout.childForceExpandHeight = false;

        LayoutElement rowLE = row.AddComponent<LayoutElement>();
        rowLE.minHeight = 220f;

        heroCard    = CreateCharacterCard(row.transform, "Hero",    "Sprites/Hero/idle/idle01.png",   16f,  CharacterSelectionData.CharacterType.Hero);
        matildaCard = CreateCharacterCard(row.transform, "Matilda", "Sprites/Matilda/Matilda_Sheet.png", 1f, CharacterSelectionData.CharacterType.Matilda);

        // Start button
        GameObject startBtn = CreateButton(characterPanel.transform, "START", StartGame);
        LayoutElement le = startBtn.AddComponent<LayoutElement>();
        le.minWidth  = 180f;
        le.minHeight = 55f;

        RefreshCardColors();
    }

    Image CreateCharacterCard(Transform parent, string charName, string spritePath, float ppu,
                              CharacterSelectionData.CharacterType type)
    {
        GameObject card = CreateObject(charName + "Card", parent);
        LayoutElement le = card.AddComponent<LayoutElement>();
        le.minWidth  = 160f;
        le.minHeight = 210f;

        Image cardImg = card.AddComponent<Image>();
        cardImg.color = unselectedColor;

        VerticalLayoutGroup vl = card.AddComponent<VerticalLayoutGroup>();
        vl.childAlignment = TextAnchor.MiddleCenter;
        vl.spacing = 10f;
        vl.padding = new RectOffset(10, 10, 10, 10);
        vl.childForceExpandWidth  = false;
        vl.childForceExpandHeight = false;

        // Portrait image
        GameObject portrait = CreateObject("Portrait", card.transform);
        LayoutElement ple = portrait.AddComponent<LayoutElement>();
        ple.minWidth  = 120f;
        ple.minHeight = 120f;
        Image portraitImg = portrait.AddComponent<Image>();
        portraitImg.preserveAspect = true;

        Sprite s = RuntimeVisualRepair.LoadSpriteRuntime(spritePath, ppu);
        if (s != null) portraitImg.sprite = s;

        CreateLabel(card.transform, charName, 20);

        // Select button inside card
        CharacterSelectionData.CharacterType captured = type;
        Image capturedCard = cardImg;
        CreateButton(card.transform, "Selecteaza", () => SelectCharacter(captured));

        return cardImg;
    }

    void SelectCharacter(CharacterSelectionData.CharacterType type)
    {
        selectedCharacter = type;
        RefreshCardColors();
    }

    void RefreshCardColors()
    {
        if (heroCard != null)
            heroCard.color = selectedCharacter == CharacterSelectionData.CharacterType.Hero
                             ? selectedColor : unselectedColor;

        if (matildaCard != null)
            matildaCard.color = selectedCharacter == CharacterSelectionData.CharacterType.Matilda
                                ? selectedColor : unselectedColor;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    static GameObject CreateObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void CreateLabel(Transform parent, string text, int fontSize)
    {
        GameObject go = new GameObject("Label_" + text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.text      = text;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize  = fontSize;
        t.color     = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight = fontSize + 10f;
    }

    static GameObject CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label + "_Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.18f, 0.28f, 0.36f, 1f);
        go.GetComponent<Button>().onClick.AddListener(action);
        CreateLabel(go.transform, label, 20);
        return go;
    }
}
