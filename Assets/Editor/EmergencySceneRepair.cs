#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EmergencySceneRepair
{
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    static EmergencySceneRepair()
    {
        EditorApplication.delayCall += RepairOpenGameScene;
    }

    [MenuItem("Tools/Survival Game/Repair Current Scene")]
    public static void RepairOpenGameScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        bool sceneWasOpenedHere = false;

        if (activeScene.path != GameScenePath && File.Exists(GameScenePath))
        {
            EditorSceneManager.OpenScene(GameScenePath);
            sceneWasOpenedHere = true;
        }

        RemoveBadStatPanel();
        RestorePlayerAnimator();
        RestorePlayerSprite();
        RestoreBackground();
        HideItemPickerUntilLevelUp();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        if (sceneWasOpenedHere)
            EditorUtility.DisplayDialog("Scene repaired", "SampleScene was repaired: hero sprite, background, and blocking stat panel.", "OK");
    }

    private static void RemoveBadStatPanel()
    {
        GameObject statPanel = GameObject.Find("StatPanel");
        if (statPanel != null)
            Object.DestroyImmediate(statPanel);
    }

    private static void RestorePlayerSprite()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        Sprite heroSprite = LoadFirstSprite("Assets/Sprites/Hero/idle/idle01.png");
        if (heroSprite != null)
            spriteRenderer.sprite = heroSprite;

        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 10;
    }

    private static void RestorePlayerAnimator()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
            animator = player.AddComponent<Animator>();

        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/HeroAnimator.controller");
        if (controller != null)
            animator.runtimeAnimatorController = controller;
    }

    private static void RestoreBackground()
    {
        if (GameObject.Find("InfiniteBackground") != null)
            return;

        Sprite grassSprite = LoadFirstSprite("Assets/Sprites/Grass_Custom_Final.png");
        if (grassSprite == null)
            grassSprite = LoadFirstSprite("Assets/Sprites/Grass_Seamless.png");
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

    private static void HideItemPickerUntilLevelUp()
    {
        GameObject picker = GameObject.Find("ItemPicker");
        if (picker != null)
            picker.SetActive(false);
    }

    private static Sprite LoadFirstSprite(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
                return sprite;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
