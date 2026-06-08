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

    private int currentXP = 0;
    private int level = 1;
    private int xpToLevelUp = 50;
    private int gold = 0;

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
        UpdateResourceUI();
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
            ShowUpgradeUI();
        }
    }

    public void AddGold(int amount)
    {
        int greedMultiplier = Mathf.Max(1, Mathf.RoundToInt(PlayerStatsRuntime.GetPercentStat(StatType.Greed, 100f)));
        gold += Mathf.RoundToInt(amount * (greedMultiplier / 100f));
        UpdateResourceUI();
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
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        if (gameOverUI != null) gameOverUI.SetActive(true);
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
        panelRect.anchorMin = new Vector2(0.28f, 0.25f);
        panelRect.anchorMax = new Vector2(0.72f, 0.75f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = runtimeUpgradeMenu.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        VerticalLayoutGroup layout = runtimeUpgradeMenu.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(25, 25, 25, 25);
        layout.spacing = 12f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateMenuText(runtimeUpgradeMenu.transform, "Level Up! Alege un upgrade", 28);

        int buttonsCreated = 0;
        foreach (ObjectSO item in availableItems)
        {
            if (item == null || Player.Instance == null || !Player.Instance.CanPickItem(item))
                continue;

            ObjectSO selectedItem = item;
            CreateMenuButton(runtimeUpgradeMenu.transform, item.objectName, () =>
            {
                Player.Instance.PickItem(selectedItem);
                ResumeGame();
            });
            buttonsCreated++;

            if (buttonsCreated >= 3)
                break;
        }

        if (buttonsCreated == 0)
        {
            CreateMenuButton(runtimeUpgradeMenu.transform, "Trage mai repede", ApplyUpgrade_FireRate);
            CreateMenuButton(runtimeUpgradeMenu.transform, "Scut orbital", ApplyUpgrade_Shield);
        }
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
