using CelikenVP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject upgradeUI;
    public GameObject gameOverUI;
    public GameObject orbitalShieldPrefab;
    public ItemPicker itemPicker;
    public List<ObjectSO> availableItems = new();
    public Text xpText;
    public Text goldText;
    private GameObject currentShield;
    private GameObject runtimeUpgradeMenu;
    private GameObject runtimeStatsPanel;

    private int currentXP = 0;
    private int level = 1;
    private int xpToLevelUp = 50;
    private int gold = 0;

    private Image xpBarFill; // bara de XP full-width sus pe ecran

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        RuntimeVisualRepair.Repair();
        if (itemPicker != null)
            itemPicker.gameObject.SetActive(false);

        // HUD compact stanga-sus: LV/XP pe randul 1, inimile pe randul 2 (in HealthUI), Gold pe randul 3
        // Stanga-sus: LV/XP (rand 1) -> inimi (rand 2, in HealthUI la y=-30) -> Gold (rand 3)
        HealthUI.PositionTopLeft(xpText,   new Vector2(12f, -18f),  14);
        HealthUI.PositionTopLeft(goldText, new Vector2(12f, -62f), 14);

        CreateXPBar();
        UpdateResourceUI();
    }

    // Bara de XP orizontala lipita de marginea de sus (stil Vampire Survivors)
    void CreateXPBar()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // Fundal bara
        GameObject bg = new GameObject("XPBar_BG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(canvas.transform, false);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(1f, 1f);
        bgRect.pivot     = new Vector2(0.5f, 1f);
        bgRect.offsetMin = new Vector2(0f, -8f);
        bgRect.offsetMax = new Vector2(0f, 0f);
        bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 0.9f);

        // Fill
        GameObject fill = new GameObject("XPBar_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(bg.transform, false);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot     = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        xpBarFill = fill.GetComponent<Image>();
        xpBarFill.color = new Color(0.3f, 0.7f, 1f, 1f); // albastru XP
    }

    public void AddXP()
    {
        AddXP(10);
    }

    public void AddXP(int amount)
    {
        int growthMultiplier = Mathf.Max(1, Mathf.RoundToInt(PlayerStatsRuntime.GetPercentStat(StatType.Growth, 100f)));
        currentXP += Mathf.RoundToInt(amount * (growthMultiplier / 100f));
        UpdateResourceUI();

        if (currentXP >= xpToLevelUp)
        {
            currentXP -= xpToLevelUp;
            level++;
            xpToLevelUp = Mathf.RoundToInt(xpToLevelUp * 1.25f);
            UpdateResourceUI();
            SoundManager.Play(SoundManager.Sfx.LevelUp);
            ShowUpgradeUI();
        }
    }

    public void AddGold(int amount)
    {
        int greedMultiplier = Mathf.Max(1, Mathf.RoundToInt(PlayerStatsRuntime.GetPercentStat(StatType.Greed, 100f)));
        gold += Mathf.RoundToInt(amount * (greedMultiplier / 100f));
        UpdateResourceUI();
    }

    // Apelat de cufarul de la miniboss - deschide alegerea de upgrade
    public void OpenUpgradeChoice()
    {
        ShowUpgradeUI();
    }

    void ShowUpgradeUI()
    {
        Time.timeScale = 0f;
        CreateRuntimeUpgradeMenu();
    }

    public void ApplyUpgrade_FireRate()
    {
        PickNamedItem("Empty Tome");
        ResumeGame();
    }

    public void ApplyUpgrade_Shield()
    {
        if (currentShield == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && orbitalShieldPrefab != null)
            {
                currentShield = Instantiate(orbitalShieldPrefab, player.transform.position, Quaternion.identity);
                currentShield.transform.SetParent(player.transform);
            }
        }
        else
        {
            currentShield.GetComponent<OrbitalShield>().rotationSpeed += 50f;
            currentShield.transform.localScale *= 1.2f;
        }
        ResumeGame();
    }

    public void FinishUpgradeSelection()
    {
        ResumeGame();
    }

    void ResumeGame()
    {
        if (upgradeUI != null) upgradeUI.SetActive(false);
        if (itemPicker != null) itemPicker.gameObject.SetActive(false);
        if (runtimeUpgradeMenu != null) Destroy(runtimeUpgradeMenu);
        if (runtimeStatsPanel != null) Destroy(runtimeStatsPanel);
        Time.timeScale = 1f;
    }

    private bool gameOverShown;

    public void GameOver()
    {
        if (gameOverShown) return;
        gameOverShown = true;
        Time.timeScale = 0f;
        if (gameOverUI != null) gameOverUI.SetActive(true);
        BuildResultsScreen();
    }

    // Ecran de rezultate (timp supravietuit, kills, gold, level)
    void BuildResultsScreen()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject panel = new GameObject("ResultsScreen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 14f;

        // Date
        HealthUI ui = FindAnyObjectByType<HealthUI>();
        int kills = ui != null ? ui.KillCount : 0;
        float t   = ui != null ? ui.SurvivalTime : 0f;
        string time = ((int)t / 60).ToString("00") + ":" + ((int)t % 60).ToString("00");

        CreateMenuText(panel.transform, "GAME OVER", 48);
        CreateMenuText(panel.transform, "Timp supravietuit:  " + time, 28);
        CreateMenuText(panel.transform, "Kills:  " + kills, 28);
        CreateMenuText(panel.transform, "Gold:  " + gold, 28);
        CreateMenuText(panel.transform, "Level:  " + level, 28);

        CreateMenuButton(panel.transform, "RESTART", RestartGame);
        CreateMenuButton(panel.transform, "MENIU", GoToMenu);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void PickNamedItem(string itemName)
    {
        if (Player.Instance == null)
            return;

        foreach (ObjectSO item in availableItems)
        {
            if (item != null && item.objectName == itemName)
            {
                Player.Instance.PickItem(item);
                return;
            }
        }
    }

    void UpdateResourceUI()
    {
        if (xpText != null)
            xpText.text = "LV " + level + "  XP " + currentXP + "/" + xpToLevelUp;
        if (goldText != null)
            goldText.text = "Gold: " + gold;
        if (xpBarFill != null)
            xpBarFill.rectTransform.anchorMax = new Vector2(
                Mathf.Clamp01((float)currentXP / Mathf.Max(1, xpToLevelUp)), 1f);
    }

    void CreateRuntimeUpgradeMenu()
    {
        if (runtimeUpgradeMenu != null)
            Destroy(runtimeUpgradeMenu);

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("RuntimeUpgradeCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        runtimeUpgradeMenu = new GameObject("RuntimeUpgradeMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        runtimeUpgradeMenu.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = runtimeUpgradeMenu.GetComponent<RectTransform>();
        // Fereastra de ~3x mai inalta (doar pe verticala), latimea ramane la fel
        panelRect.anchorMin = new Vector2(0.35f, 0.05f);
        panelRect.anchorMax = new Vector2(0.65f, 0.95f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = runtimeUpgradeMenu.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        VerticalLayoutGroup layout = runtimeUpgradeMenu.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 10, 10);
        layout.spacing = 6f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateMenuText(runtimeUpgradeMenu.transform, "Level Up! Alege un upgrade", 18);

        // Shuffle ca fiecare level-up sa arate iteme diferite (ca in Vampire Survivors)
        var pool = new List<ObjectSO>(availableItems);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // Revolver (proiectil extra) doar pentru personajele care trag proiectile (Hero, Countess)
        var sel = CharacterSelectionData.Selected;
        bool isShooter = sel == CharacterSelectionData.CharacterType.Hero
                      || sel == CharacterSelectionData.CharacterType.Countess_Vampire;

        int buttonsCreated = 0;
        foreach (ObjectSO item in pool)
        {
            if (item == null || Player.Instance == null)
                continue;

            if (item.objectName == "Revolver" && !isShooter)
                continue;

            // Itemul si-a atins nivelul maxim (stack 5) -> in locul lui apare +25 gold
            bool maxed = Player.Instance.GetCurrentItemLevel(item) >= item.objectMaxLevel;
            if (maxed)
            {
                CreateGoldRewardButton(runtimeUpgradeMenu.transform, 25);
                buttonsCreated++;
            }
            else if (Player.Instance.CanPickItem(item))
            {
                ObjectSO selectedItem = item;
                CreateItemButton(runtimeUpgradeMenu.transform, selectedItem, () =>
                {
                    Player.Instance.PickItem(selectedItem);
                    ApplyItemSpecialEffects(selectedItem);
                    ResumeGame();
                });
                buttonsCreated++;
            }

            if (buttonsCreated >= 3)
                break;
        }

        if (buttonsCreated == 0)
        {
            CreateMenuButton(runtimeUpgradeMenu.transform, "Trage mai repede", ApplyUpgrade_FireRate);
            CreateMenuButton(runtimeUpgradeMenu.transform, "Scut orbital", ApplyUpgrade_Shield);
        }

        CreateStatsSidePanel(canvas);
    }

    // Buton de recompensa: +N gold, cu imaginea saculetului (inlocuieste un item maxat)
    void CreateGoldRewardButton(Transform parent, int amount)
    {
        GameObject buttonObject = new GameObject("Gold Reward Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        buttonObject.GetComponent<Image>().color = new Color(0.30f, 0.26f, 0.10f, 1f);
        buttonObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            AddGold(amount);
            ResumeGame();
        });

        Sprite bag = RuntimeVisualRepair.LoadSpriteRuntime("CurrencyGems/MoneyBag/MoneyBag.png", 32f);
        if (bag != null)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(buttonObject.transform, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot     = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(26f, 26f);

            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = bag;
            iconImage.preserveAspect = true;
        }

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(44f, 2f);
        textRect.offsetMax = new Vector2(-8f, -2f);

        Text label = textObject.GetComponent<Text>();
        label.text      = "+" + amount + " gold";
        label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize  = 16;
        label.fontStyle = FontStyle.Bold;
        label.color     = new Color(1f, 0.92f, 0.5f, 1f);
        label.alignment = TextAnchor.MiddleLeft;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
    }

    // Buton de upgrade cu iconita itemului + nume + efect (ex. "Beer  +1hp").
    // Pozitionare manuala (fara layout group) ca textul sa fie mereu vizibil.
    void CreateItemButton(Transform parent, ObjectSO item, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(item.objectName + " Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        buttonObject.GetComponent<Image>().color = new Color(0.18f, 0.28f, 0.36f, 1f);
        buttonObject.GetComponent<Button>().onClick.AddListener(action);

        // Iconita - ancorata in stanga butonului
        Sprite icon = RuntimeVisualRepair.LoadSpriteRuntime(IconPath(item), 32f);
        if (icon == null) icon = item.objectIcon;
        if (icon != null)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(buttonObject.transform, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot     = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(26f, 26f);

            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
        }

        // Eticheta "Nume   efect" - umple restul butonului, aliniata la stanga
        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(44f, 2f);
        textRect.offsetMax = new Vector2(-8f, -2f);

        Text label = textObject.GetComponent<Text>();
        label.text      = item.objectName + "   " + item.objectDescription;
        label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize  = 15;
        label.color     = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow   = VerticalWrapMode.Overflow;
    }

    // Calea iconitei pentru fiecare item (numele fisierului difera de objectName la cateva)
    string IconPath(ObjectSO item)
    {
        switch (item.objectName)
        {
            case "Luck":          return "Sprites/Items/Dice-1.png";
            case "Book of Faith": return "Sprites/Items/Bible-1.png";
            case "Dagger":        return "Sprites/Items/Dagger-1.png";
            case "Heart":         return "Sprites/Items/Heart-1.png";
            case "Revolver":      return "Sprites/Items/Revolver-1.png";
            default:              return "Sprites/Items/" + item.objectName + "-1.png";
        }
    }

    // Efecte care nu trec prin sistemul de stats
    // (Beer = inima noua, Shield = scut pe kill-uri, Book of Faith = carte rotativa)
    void ApplyItemSpecialEffects(ObjectSO item)
    {
        if (item == null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        if (item.objectName == "Beer")
        {
            PlayerController controller = playerObject.GetComponent<PlayerController>();
            if (controller != null) controller.AddHeart();
        }
        else if (item.objectName == "Shield")
        {
            ShieldSystem shield = playerObject.GetComponent<ShieldSystem>();
            if (shield == null) shield = playerObject.AddComponent<ShieldSystem>();

            int level = Player.Instance != null ? Player.Instance.GetCurrentItemLevel(item) : 1;
            shield.SetItemLevel(Mathf.Max(1, level));
        }
        else if (item.objectName == "Book of Faith")
        {
            OrbitingBooks books = playerObject.GetComponent<OrbitingBooks>();
            if (books == null) books = playerObject.AddComponent<OrbitingBooks>();
            books.AddBook(item.objectMaxLevel);
        }
        else if (item.objectName == "Heart")
        {
            // Reduce intervalul de regen cu 1s si creste cantitatea cu 0.5 inima (1 HP intern)
            PlayerController controller = playerObject.GetComponent<PlayerController>();
            if (controller != null) controller.BoostRecovery(1f, 1);
        }
    }

    // Bara verticala de stats, lipita de marginea stanga a ecranului, vizibila la level-up
    void CreateStatsSidePanel(Canvas canvas)
    {
        if (runtimeStatsPanel != null)
            Destroy(runtimeStatsPanel);

        runtimeStatsPanel = new GameObject("RuntimeStatsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        runtimeStatsPanel.transform.SetParent(canvas.transform, false);

        RectTransform rect = runtimeStatsPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.22f);
        rect.anchorMax = new Vector2(0.18f, 0.78f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        runtimeStatsPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        VerticalLayoutGroup layout = runtimeStatsPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 3f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;

        // Valorile curente: pornesc din profilul personajului + bonusurile din iteme
        CharacterProfile.Profile profile = CharacterProfile.Current;

        int hearts = profile.hearts;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            PlayerController controller = playerObject.GetComponent<PlayerController>();
            if (controller != null) hearts = controller.maxHealth / 2;
        }

        // Viteza: profilul (5% Hero, 7% Matilda) + bonusurile din MoveSpeed (Feather +10%)
        float moveSpeedPercent = PlayerStatsRuntime.GetPercentStat(StatType.MoveSpeed, 100f);
        int displaySpeed = profile.speedPercent + Mathf.RoundToInt(moveSpeedPercent - 100f);

        int shieldKills = profile.shieldKills;
        if (ShieldSystem.Instance != null && ShieldSystem.Instance.HasItem)
            shieldKills = ShieldSystem.Instance.KillsRequired;

        // Luck: din profil + bonusuri; Damage: din profil scalat cu Might
        int displayLuck   = profile.luckPercent + Mathf.RoundToInt(PlayerStatsRuntime.GetPercentStat(StatType.Luck, 100f) - 100f);
        int displayDamage = Mathf.RoundToInt(profile.damage * PlayerStatsRuntime.GetMultiplier(StatType.Might));

        // Recovery live din PlayerController (reflecta upgrade-urile Heart)
        float recAmount   = profile.recoveryAmount;
        float recInterval = profile.recoveryInterval;
        if (playerObject != null)
        {
            PlayerController pcr = playerObject.GetComponent<PlayerController>();
            if (pcr != null) { recAmount = pcr.RecoveryAmount; recInterval = pcr.RecoveryInterval; }
        }
        float recoveryHearts = recAmount * 0.5f;
        string recoveryLine  = "- recovery " + recoveryHearts.ToString("0.#") + " / " + recInterval.ToString("0") + "s";

        CreateStatText(runtimeStatsPanel.transform, "STATS:", 13);
        CreateStatText(runtimeStatsPanel.transform, "- health " + hearts + "hp", 11);
        CreateStatText(runtimeStatsPanel.transform, "- speed " + displaySpeed + "%", 11);
        CreateStatText(runtimeStatsPanel.transform, "- shield " + shieldKills + " kills", 11);
        CreateStatText(runtimeStatsPanel.transform, "- luck " + displayLuck + "%", 11);
        CreateStatText(runtimeStatsPanel.transform, "- damage " + displayDamage, 11);
        CreateStatText(runtimeStatsPanel.transform, recoveryLine, 11);
    }

    void CreateStatText(Transform parent, string text, int fontSize)
    {
        GameObject textObject = new GameObject("Stat", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text label = textObject.GetComponent<Text>();
        label.text      = text;
        label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize  = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.color     = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = fontSize + 8f;
    }

    void CreateMenuText(Transform parent, string text, int fontSize)
    {
        GameObject textObject = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text label = textObject.GetComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
    }

    void CreateMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.28f, 0.36f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        CreateMenuText(buttonObject.transform, label, 22);
    }
}
