using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class RuntimeVisualRepair
{
    public static void Repair()
    {
        RestorePlayerSpriteIfMissing();
        RestoreBackgroundIfMissing();
        EnsureHealthUI();
    }

    public static HealthUI EnsureHealthUI()
    {
        HealthUI existing = Object.FindAnyObjectByType<HealthUI>();
        if (existing != null)
            return existing;

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject healthObject = new GameObject("HealthUI");
        healthObject.transform.SetParent(canvas.transform, false);
        HealthUI healthUI = healthObject.AddComponent<HealthUI>();

        Sprite fullHeart = LoadSpriteFromAssets("Sprites/UI/Heart_Full_64.png", 64f);
        Sprite halfHeart = LoadSpriteFromAssets("Sprites/UI/Heart_Half_64.png", 64f);
        Sprite emptyHeart = LoadSpriteFromAssets("Sprites/UI/Heart_Empty_64.png", 64f);

        healthUI.fullHeart = fullHeart;
        healthUI.halfHeart = halfHeart;
        healthUI.emptyHeart = emptyHeart;
        healthUI.heartImages = new Image[3];

        for (int i = 0; i < healthUI.heartImages.Length; i++)
        {
            GameObject heart = new GameObject("Heart_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            heart.transform.SetParent(canvas.transform, false);

            RectTransform rect = heart.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(16f + i * 42f, -16f);
            rect.sizeDelta = new Vector2(36f, 36f);

            Image image = heart.GetComponent<Image>();
            image.sprite = fullHeart;
            image.preserveAspect = true;
            healthUI.heartImages[i] = image;
        }

        return healthUI;
    }

    private static void RestorePlayerSpriteIfMissing()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite != null)
            return;

        Sprite heroSprite = LoadSpriteFromAssets("Sprites/Hero/idle/idle01.png", 16f);
        if (heroSprite != null)
            spriteRenderer.sprite = heroSprite;
    }

    private static void RestoreBackgroundIfMissing()
    {
        if (GameObject.Find("InfiniteBackground") != null)
            return;

        Sprite grassSprite = LoadSpriteFromAssets("Sprites/Grass_Custom_Final.png", 100f);
        if (grassSprite == null)
            grassSprite = LoadSpriteFromAssets("Sprites/Grass_Seamless.png", 32f);
        if (grassSprite == null)
            return;

        GameObject background = new GameObject("InfiniteBackground");
        float chunkSize = 60f;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                GameObject chunk = new GameObject("GrassChunk_" + x + "_" + y);
                chunk.transform.SetParent(background.transform);
                chunk.transform.position = new Vector3(x * chunkSize, y * chunkSize, 0f);

                SpriteRenderer renderer = chunk.AddComponent<SpriteRenderer>();
                renderer.sprite = grassSprite;
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(chunkSize, chunkSize);
                renderer.sortingOrder = -100;
            }
        }

        InfiniteBackground infiniteBackground = background.AddComponent<InfiniteBackground>();
        infiniteBackground.chunkSize = chunkSize;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            infiniteBackground.player = player.transform;
    }

    public static Sprite LoadSpriteRuntime(string relativePath, float pixelsPerUnit)
        => LoadSpriteFromAssets(relativePath, pixelsPerUnit);

    private static Sprite LoadSpriteFromAssets(string relativePath, float pixelsPerUnit)
    {
        string path = Path.Combine(Application.dataPath, relativePath);
        if (!File.Exists(path))
            return null;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        if (!texture.LoadImage(bytes))
            return null;

        Rect rect = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }
}
