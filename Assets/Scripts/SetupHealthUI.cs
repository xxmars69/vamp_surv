using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

#if UNITY_EDITOR
public class SetupHealthUI : MonoBehaviour
{
    [MenuItem("Joc 2D/Creeaza Sistem UI Complet")]
    public static void Setup()
    {
        Sprite full = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Full_64.png");
        Sprite half = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Half_64.png");
        Sprite empty = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Empty_64.png");

        GameObject canvasObj = GameObject.Find("UI_Canvas");
        if (canvasObj != null) DestroyImmediate(canvasObj);
        
        canvasObj = new GameObject("UI_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        Font mainFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (mainFont == null) mainFont = (Font)Resources.FindObjectsOfTypeAll(typeof(Font))[0];

        // 1. INIMI (Stanga Sus)
        GameObject container = new GameObject("HeartContainer");
        container.transform.SetParent(canvasObj.transform);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(15, -15);
        containerRect.sizeDelta = new Vector2(150, 40);

        HealthUI healthUI = container.AddComponent<HealthUI>();
        healthUI.fullHeart = full;
        healthUI.halfHeart = half;
        healthUI.emptyHeart = empty;
        healthUI.heartImages = new Image[3];

        for (int i = 0; i < 3; i++)
        {
            GameObject heartObj = new GameObject("Heart_" + i);
            heartObj.transform.SetParent(container.transform);
            RectTransform hRect = heartObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 0.5f);
            hRect.anchorMax = new Vector2(0, 0.5f);
            hRect.pivot = new Vector2(0, 0.5f);
            hRect.anchoredPosition = new Vector2(i * 25, 0); 
            hRect.sizeDelta = new Vector2(20, 20);
            Image img = heartObj.AddComponent<Image>();
            img.sprite = full;
            healthUI.heartImages[i] = img;
        }

        // 2. KILL COUNTER (Sub Inimi)
        GameObject killObj = new GameObject("KillCounter");
        killObj.transform.SetParent(canvasObj.transform);
        Text killText = killObj.AddComponent<Text>();
        killText.text = "Kills: 0";
        killText.font = mainFont;
        killText.fontSize = 15;
        killText.color = Color.white;
        RectTransform killRect = killObj.GetComponent<RectTransform>();
        killRect.anchorMin = new Vector2(0, 1);
        killRect.anchorMax = new Vector2(0, 1);
        killRect.pivot = new Vector2(0, 1);
        killRect.anchoredPosition = new Vector2(15, -45);
        killRect.sizeDelta = new Vector2(200, 40);
        healthUI.killText = killText;

        // 3. TIMER (Centru Sus)
        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(canvasObj.transform);
        Text timerText = timerObj.AddComponent<Text>();
        timerText.text = "00:00";
        timerText.font = mainFont;
        timerText.fontSize = 15;
        timerText.color = Color.white;
        timerText.alignment = TextAnchor.UpperCenter;
        RectTransform timerRect = timerObj.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1);
        timerRect.anchorMax = new Vector2(0.5f, 1);
        timerRect.pivot = new Vector2(0.5f, 1);
        timerRect.anchoredPosition = new Vector2(0, -15);
        timerRect.sizeDelta = new Vector2(200, 50);
        healthUI.timerText = timerText;

        // 4. ECRAN GAME OVER (Ascuns)
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform);
        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        gameOverPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);

        GameObject goText = new GameObject("GameOverText");
        goText.transform.SetParent(gameOverPanel.transform);
        Text t = goText.AddComponent<Text>();
        t.text = "GAME OVER";
        t.font = mainFont;
        t.fontSize = 70;
        t.color = Color.red;
        t.alignment = TextAnchor.MiddleCenter;
        goText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
        goText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);

        GameObject restartText = new GameObject("RestartText");
        restartText.transform.SetParent(gameOverPanel.transform);
        Text rt = restartText.AddComponent<Text>();
        rt.text = "Apasă R pentru RESTART";
        rt.font = mainFont;
        rt.fontSize = 25;
        rt.color = Color.white;
        rt.alignment = TextAnchor.MiddleCenter;
        restartText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
        restartText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 50);

        healthUI.gameOverPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        Debug.Log("UI Complet Creat: Inimi, Kill Counter si Timer!");
    }
}
#endif
