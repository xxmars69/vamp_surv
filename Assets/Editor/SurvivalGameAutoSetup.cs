#if UNITY_EDITOR
using CelikenVP;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SurvivalGameAutoSetup
{
    private const string MarkerPath = "ProjectSettings/SurvivalGameAutoSetup.done";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string PlayerStatsPath = "Assets/VampireSystem/ScriptableObjects/PlayerStats.asset";
    private const string ObjectFolderPath = "Assets/VampireSystem/ScriptableObjects/Objects";

    [MenuItem("Tools/Survival Game/Run Auto Setup")]
    public static void RunManual()
    {
        RunSetup();
    }

    private static void RunOnce()
    {
        if (File.Exists(MarkerPath))
            return;

        RunSetup();
        File.WriteAllText(MarkerPath, "SurvivalGameAutoSetup completed.");
    }

    private static void RunSetup()
    {
        AssetDatabase.Refresh();
        ConfigureImportedSprites();

        GameObject blueGem = CreatePickupPrefab("BlueXPGem", "Assets/CurrencyGems/Gems/BlueDiamond.png", CollectibleKind.Experience, 10, 0.45f);
        GameObject redGem = CreatePickupPrefab("RedXPGem", "Assets/CurrencyGems/Gems/RedDiamond.png", CollectibleKind.Experience, 50, 0.5f);
        GameObject goldCoin = CreatePickupPrefab("GoldCoin", "Assets/CurrencyGems/Gold/GoldCoin.png", CollectibleKind.Gold, 1, 0.45f);
        GameObject platinumCoin = CreatePickupPrefab("PlatinumCoin", "Assets/CurrencyGems/Platinum/PlatinumCoin.png", CollectibleKind.Gold, 10, 0.45f);
        GameObject moneyBag = CreatePickupPrefab("MoneyBag", "Assets/CurrencyGems/MoneyBag/MoneyBag.png", CollectibleKind.Gold, 50, 0.65f);

        ConfigureEnemyPrefab(blueGem, redGem);
        ConfigureGameScene(blueGem, redGem, goldCoin, platinumCoin, moneyBag);
        CreateMainMenuScene();
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigureImportedSprites()
    {
        string[] spritePaths =
        {
            "Assets/CurrencyGems/Gems/BlueDiamond.png",
            "Assets/CurrencyGems/Gems/RedDiamond.png",
            "Assets/CurrencyGems/Gold/GoldCoin.png",
            "Assets/CurrencyGems/Platinum/PlatinumCoin.png",
            "Assets/CurrencyGems/MoneyBag/MoneyBag.png"
        };

        foreach (string path in spritePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static GameObject CreatePickupPrefab(string prefabName, string spritePath, CollectibleKind kind, int value, float scale)
    {
        string prefabPath = $"Assets/Prefabs/{prefabName}.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
            return existingPrefab;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        GameObject go = new GameObject(prefabName);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;

        CollectiblePickup pickup = go.AddComponent<CollectiblePickup>();
        pickup.kind = kind;
        pickup.value = value;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void ConfigureEnemyPrefab(GameObject blueGem, GameObject redGem)
    {
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        if (enemyPrefab == null)
            return;

        GameObject instance = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
        Enemy enemy = instance.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.blueXpGemPrefab = blueGem;
            enemy.redXpGemPrefab = redGem;
        }

        PrefabUtility.SaveAsPrefabAsset(instance, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(instance);
    }

    private static void ConfigureGameScene(GameObject blueGem, GameObject redGem, GameObject goldCoin, GameObject platinumCoin, GameObject moneyBag)
    {
        if (!File.Exists(GameScenePath))
            return;

        EditorSceneManager.OpenScene(GameScenePath);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        Player player = null;
        if (playerObject != null)
        {
            player = playerObject.GetComponent<Player>();
            if (player == null)
                player = playerObject.AddComponent<Player>();
            SetSerializedObjectReference(player, "playerStats", AssetDatabase.LoadAssetAtPath<PlayerStatsSO>(PlayerStatsPath));
        }

        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.availableItems = LoadCoreItems();
            gameManager.itemPicker = EnsureItemPicker();
            EnsureResourceText(gameManager);

            WorldCurrencySpawner spawner = gameManager.GetComponent<WorldCurrencySpawner>();
            if (spawner == null)
                spawner = gameManager.gameObject.AddComponent<WorldCurrencySpawner>();
            spawner.goldCoinPrefab = goldCoin;
            spawner.platinumCoinPrefab = platinumCoin;
            spawner.moneyBagPrefab = moneyBag;
            if (playerObject != null)
                spawner.player = playerObject.transform;
        }

        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy.blueXpGemPrefab = blueGem;
            enemy.redXpGemPrefab = redGem;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static ItemPicker EnsureItemPicker()
    {
        ItemPicker existing = Object.FindFirstObjectByType<ItemPicker>(FindObjectsInactive.Include);
        if (existing != null)
            return existing;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas("UI_Canvas");

        GameObject panel = CreatePanel("ItemPicker", canvas.transform, new Color(0f, 0f, 0f, 0.78f));
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0.2f);
        rect.anchorMax = new Vector2(0.75f, 0.8f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.08f, 0.08f);
        contentRect.anchorMax = new Vector2(0.92f, 0.92f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ItemPicker picker = panel.AddComponent<ItemPicker>();
        SetSerializedObjectReference(picker, "parentContent", content.transform);
        SetSerializedObjectReference(picker, "pfObjectRow", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VampireSystem/Prefabs/pfObjectRow.prefab"));
        panel.SetActive(false);
        return picker;
    }

    private static void EnsureStatPanel()
    {
        if (Object.FindFirstObjectByType<StatDisplayer>(FindObjectsInactive.Include) != null)
            return;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas("UI_Canvas");

        GameObject panel = CreatePanel("StatPanel", canvas.transform, new Color(0f, 0f, 0f, 0.45f));
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.78f, 0.48f);
        rect.anchorMax = new Vector2(0.98f, 0.95f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.08f, 0.08f);
        contentRect.anchorMax = new Vector2(0.92f, 0.92f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        StatDisplayer displayer = panel.AddComponent<StatDisplayer>();
        SetSerializedObjectReference(displayer, "playerStats", AssetDatabase.LoadAssetAtPath<PlayerStatsSO>(PlayerStatsPath));
        SetSerializedObjectReference(displayer, "parentContent", content.transform);
        SetSerializedObjectReference(displayer, "pfStatLine", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VampireSystem/Prefabs/pfStatLine.prefab"));
    }

    private static void EnsureResourceText(GameManager gameManager)
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas("UI_Canvas");

        Text xpText = FindOrCreateText("XPText", canvas.transform, "LV 1  XP 0/50", new Vector2(0.02f, 0.93f), new Vector2(0.35f, 0.99f));
        Text goldText = FindOrCreateText("GoldText", canvas.transform, "Gold: 0", new Vector2(0.02f, 0.87f), new Vector2(0.25f, 0.93f));
        gameManager.xpText = xpText;
        gameManager.goldText = goldText;
    }

    private static List<ObjectSO> LoadCoreItems()
    {
        string[] names = { "Spinach", "Wings", "Armor", "Empty Tome" };
        List<ObjectSO> items = new();
        foreach (string itemName in names)
        {
            ObjectSO item = AssetDatabase.LoadAssetAtPath<ObjectSO>($"{ObjectFolderPath}/{itemName}.asset");
            if (item != null)
                items.Add(item);
        }
        return items;
    }

    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f);

        Canvas canvas = CreateCanvas("MainMenuCanvas");
        MainMenuController controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();

        GameObject mainPanel = CreatePanel("MainPanel", canvas.transform, new Color(0f, 0f, 0f, 0f));
        GameObject characterPanel = CreatePanel("CharacterSelectionPanel", canvas.transform, new Color(0f, 0f, 0f, 0.45f));
        GameObject creditsPanel = CreatePanel("CreditsPanel", canvas.transform, new Color(0f, 0f, 0f, 0.55f));

        controller.mainPanel = mainPanel;
        controller.characterPanel = characterPanel;
        controller.creditsPanel = creditsPanel;

        CreateText("Title", mainPanel.transform, "2D Survival", 46, new Vector2(0.25f, 0.68f), new Vector2(0.75f, 0.82f));
        CreateButton("PlayButton", mainPanel.transform, "PLAY", new Vector2(0.4f, 0.52f), new Vector2(0.6f, 0.61f), controller.ShowCharacterSelection);
        CreateButton("SettingsButton", mainPanel.transform, "SETTINGS", new Vector2(0.4f, 0.4f), new Vector2(0.6f, 0.49f), controller.SettingsPlaceholder);
        CreateButton("CreditsButton", mainPanel.transform, "CREDITS", new Vector2(0.4f, 0.28f), new Vector2(0.6f, 0.37f), controller.ShowCredits);

        CreateText("CharacterTitle", characterPanel.transform, "Character Selection", 34, new Vector2(0.25f, 0.68f), new Vector2(0.75f, 0.8f));
        CreateButton("OnlyCharacterButton", characterPanel.transform, "Marius Survivor", new Vector2(0.36f, 0.48f), new Vector2(0.64f, 0.58f), controller.StartGame);
        CreateButton("CharacterBackButton", characterPanel.transform, "BACK", new Vector2(0.42f, 0.32f), new Vector2(0.58f, 0.4f), controller.BackToMain);

        CreateText("CreditsText", creditsPanel.transform, "creatorii mei Marius Mihai si Andrei sunt baieti foarte de treaba, #friendship foreva, puteti sa le donati un banut sau doi, ar fi foarte apreciat", 24, new Vector2(0.18f, 0.42f), new Vector2(0.82f, 0.68f));
        CreateButton("CreditsBackButton", creditsPanel.transform, "BACK", new Vector2(0.42f, 0.25f), new Vector2(0.58f, 0.33f), controller.BackToMain);

        characterPanel.SetActive(false);
        creditsPanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        if (Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        return canvas;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    private static Text FindOrCreateText(string name, Transform parent, string value, Vector2 anchorMin, Vector2 anchorMax)
    {
        Transform existing = parent.Find(name);
        if (existing != null && existing.TryGetComponent(out Text existingText))
            return existingText;
        return CreateText(name, parent, value, 24, anchorMin, anchorMax);
    }

    private static Text CreateText(string name, Transform parent, string value, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.22f, 0.26f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, action);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CreateText("Text", buttonObject.transform, label, 24, Vector2.zero, Vector2.one);
        return button;
    }

    private static void SetSerializedObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
    }
}
#endif
