#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Automatizeaza pasii din tutorialul Terresquall "Rogue-like Vampire Survivors Part 2":
// slicing terrain 32x32, Tile assets, Tilemap pictat 20x20, prefab-uri Terrain Chunk
// (cu spawn points + collider + ChunkTrigger + PropRandomizer), layer Terrain,
// si MapController in scena - tot ce in tutorial se face manual in Editor.
//
// Rulare: meniul  Tools > VS Tutorial > Setup Terrain System
public static class VSTerrainSetup
{
    const string TerrainPath = "Assets/Sprites/Terrain/terrain.png";
    const string GameScenePath = "Assets/Scenes/SampleScene.unity";
    const int TILE = 32;          // px per tile
    const int TEX_W = 672, TEX_H = 736;
    const int COLS = TEX_W / TILE; // 21
    const int ROWS = TEX_H / TILE; // 23
    const int CHUNK = 20;          // tile-uri per chunk (20x20)
    const int TerrainLayer = 6;    // User Layer 6 = Terrain (ca in tutorial)

    // Iarba cu detalii: plat (1,9) + 2 variatii cu fire de iarba (1,11),(2,11).
    // Asamblate intr-o singura textura mare per chunk => detalii vizibile, ZERO linii.
    static readonly (int c, int r)[] GrassTiles = { (1, 9), (1, 9), (1, 11), (2, 11) };

    // Fara props (utilizatorul le-a cerut scoase complet)
    static readonly (int c, int r, string name)[] PropTiles = { };

    [MenuItem("Tools/VS Tutorial/Setup Terrain System")]
    public static void SetupAll()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Iesi din Play Mode",
                "Setup-ul de teren modifica scene si asset-uri. Trebuie sa fii in Edit Mode.\n\n" +
                "Apasa Stop (butonul Play din toolbar) si re-incearca.", "OK");
            return;
        }

        if (!System.IO.File.Exists(TerrainPath))
        {
            EditorUtility.DisplayDialog("Eroare", "Nu gasesc " + TerrainPath, "OK");
            return;
        }

        EnsureFolder("Assets/Tiles");
        EnsureFolder("Assets/Prefabs/Terrain");

        SetTerrainLayerName();
        DisableAntiAliasing(); // fix-ul din tutorial pentru liniile dintre tile-uri
        SliceTerrain();

        Tile[] grass = CreateGrassTiles();
        List<GameObject> propPrefabs = CreatePropPrefabs();

        // 3 variante de chunk (tutorialul cere 3 variatii)
        List<GameObject> chunkPrefabs = new();
        for (int i = 1; i <= 3; i++)
            chunkPrefabs.Add(BuildChunkPrefab("TerrainChunk_" + i, grass, propPrefabs, seed: i));

        SetupSceneMapController(chunkPrefabs);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Gata",
            "Sistemul de teren a fost configurat in SampleScene (scena de joc):\n" +
            "- terrain.png taiat 32x32\n" +
            "- 3 prefab-uri TerrainChunk (in Assets/Prefabs/Terrain)\n" +
            "- Map Controller + chunk initial in SampleScene\n" +
            "- fundalul vechi (InfiniteBackground) dezactivat\n" +
            "- obiectele ramase gresit in MainMenu au fost sterse\n\n" +
            "SampleScene e deja deschisa si salvata. Apasa Play.", "OK");
    }

    // Dezactiveaza Anti-Aliasing in toate quality levels (tutorialul cere asta
    // ca fix pentru liniile dintre tile-uri). Afecteaza si MSAA din URP asset.
    static void DisableAntiAliasing()
    {
        int original = QualitySettings.GetQualityLevel();
        string[] names = QualitySettings.names;
        for (int i = 0; i < names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.antiAliasing = 0;
        }
        QualitySettings.SetQualityLevel(original, false);
    }

    // ── 1. Layer Terrain ─────────────────────────────────────────────────
    static void SetTerrainLayerName()
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        if (layers != null && layers.arraySize > TerrainLayer)
        {
            var el = layers.GetArrayElementAtIndex(TerrainLayer);
            if (string.IsNullOrEmpty(el.stringValue))
                el.stringValue = "Terrain";
            tagManager.ApplyModifiedProperties();
        }
    }

    // ── 2. Slicing 32x32 (Sprite Mode Multiple, PPU 32, Point) ──────────
    static void SliceTerrain()
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(TerrainPath);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = TILE;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.spriteBorder = Vector4.zero;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.isReadable = true; // necesar pentru GetPixels la asamblarea texturii de chunk
        // Read/Write enabled - necesar pentru GetPixels in BuildChunkGroundSprite
        ti.isReadable = true;

        var metas = new List<SpriteMetaData>();
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
            {
                metas.Add(new SpriteMetaData
                {
                    name = $"terrain_{c}_{r}",
                    rect = new Rect(c * TILE, TEX_H - (r + 1) * TILE, TILE, TILE),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
            }

#pragma warning disable CS0618
        ti.spritesheet = metas.ToArray();
#pragma warning restore CS0618
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
    }

    // ── 3. Tile assets din tile-urile de iarba ──────────────────────────
    static Tile[] CreateGrassTiles()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(TerrainPath)
            .OfType<Sprite>().ToDictionary(s => s.name);

        var tiles = new List<Tile>();
        foreach (var (c, r) in GrassTiles)
        {
            string sprName = $"terrain_{c}_{r}";
            if (!sprites.TryGetValue(sprName, out var spr)) continue;

            string path = $"Assets/Tiles/{sprName}.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = spr;
                AssetDatabase.CreateAsset(tile, path);
            }
            else tile.sprite = spr;
            tiles.Add(tile);
        }
        return tiles.ToArray();
    }

    // Asambleaza tile-urile de iarba intr-o singura textura 640x640 (20x20),
    // salvata ca PNG asset. Un singur sprite => fara linii intre tile-uri.
    static Sprite BuildChunkGroundSprite(Tile[] grass, System.Random rng, int seed)
    {
        if (grass == null || grass.Length == 0) return null;

        int W = CHUNK * TILE; // 640
        Texture2D combined = new Texture2D(W, W, TextureFormat.RGBA32, false);
        combined.filterMode = FilterMode.Point;
        combined.wrapMode = TextureWrapMode.Clamp;

        for (int gx = 0; gx < CHUNK; gx++)
            for (int gy = 0; gy < CHUNK; gy++)
            {
                Tile t = grass[rng.Next(grass.Length)];
                if (t == null || t.sprite == null) continue;
                Rect r = t.sprite.textureRect;
                Color[] px = t.sprite.texture.GetPixels(
                    Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y), TILE, TILE);
                combined.SetPixels(gx * TILE, gy * TILE, TILE, TILE, px);
            }
        combined.Apply();

        string pngPath = $"Assets/Sprites/Terrain/GroundChunk_{seed}.png";
        System.IO.File.WriteAllBytes(pngPath, combined.EncodeToPNG());
        Object.DestroyImmediate(combined);
        AssetDatabase.ImportAsset(pngPath);

        var ti = (TextureImporter)AssetImporter.GetAtPath(pngPath);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = TILE;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
    }

    // Prefab-uri de decoruri (pietre, tufisuri) din tile-urile cu transparenta
    static List<GameObject> CreatePropPrefabs()
    {
        EnsureFolder("Assets/Prefabs/Props");
        var sprites = AssetDatabase.LoadAllAssetsAtPath(TerrainPath)
            .OfType<Sprite>().ToDictionary(s => s.name);

        var prefabs = new List<GameObject>();
        foreach (var (c, r, propName) in PropTiles)
        {
            string spr = $"terrain_{c}_{r}";
            if (!sprites.TryGetValue(spr, out var sprite)) continue;

            string path = $"Assets/Prefabs/Props/{propName}.prefab";
            GameObject go = new GameObject(propName);
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f); // pietre mici, decorative
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -50; // peste teren, sub jucator

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            prefabs.Add(prefab);
        }
        return prefabs;
    }

    // ── 4-6. Prefab Terrain Chunk complet ───────────────────────────────
    static GameObject BuildChunkPrefab(string name, Tile[] grass, List<GameObject> propPrefabs, int seed)
    {
        var rng = new System.Random(seed);

        GameObject root = new GameObject(name);
        root.layer = TerrainLayer;

        // BACKGROUND: o singura textura mare 640x640 asamblata din tile-uri de iarba.
        // Un singur sprite = zero cusaturi/linii intre tile-uri.
        Sprite groundSprite = BuildChunkGroundSprite(grass, rng, seed);
        if (groundSprite != null)
        {
            GameObject bg = new GameObject("Ground");
            bg.transform.SetParent(root.transform, false);
            bg.layer = TerrainLayer;
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = groundSprite;
            bgSr.sortingOrder = -100;
        }

        // Collider trigger 18x18 (ca in tutorial) pe layer-ul Terrain
        var col = root.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(CHUNK - 2, CHUNK - 2);

        // Cele 8 puncte de spawn (la +-20)
        AddPoint(root, "Right", CHUNK, 0);
        AddPoint(root, "Left", -CHUNK, 0);
        AddPoint(root, "Up", 0, CHUNK);
        AddPoint(root, "Down", 0, -CHUNK);
        AddPoint(root, "Right Up", CHUNK, CHUNK);
        AddPoint(root, "Right Down", CHUNK, -CHUNK);
        AddPoint(root, "Left Up", -CHUNK, CHUNK);
        AddPoint(root, "Left Down", -CHUNK, -CHUNK);

        // Props + Prop Locations (distribuite uniform)
        GameObject props = new GameObject("Props");
        props.transform.SetParent(root.transform, false);
        var spawnPoints = new List<GameObject>();
        for (int gx = -1; gx <= 1; gx++)
            for (int gy = -1; gy <= 1; gy++)
            {
                GameObject loc = new GameObject("Prop Location");
                loc.transform.SetParent(props.transform, false);
                loc.transform.localPosition = new Vector3(gx * 6f, gy * 6f, 0f);
                spawnPoints.Add(loc);
            }

        // ChunkTrigger + PropRandomizer
        var ct = root.AddComponent<ChunkTrigger>();
        ct.targetMap = root; // self-reference (se remapeaza la instantiere)

        var pr = root.AddComponent<PropRandomizer>();
        pr.propSpawnPoints = spawnPoints;
        pr.propPrefabs = new List<GameObject>(propPrefabs);

        // Salveaza ca prefab
        string prefabPath = $"Assets/Prefabs/Terrain/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void AddPoint(GameObject parent, string name, float x, float y)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(parent.transform, false);
        p.transform.localPosition = new Vector3(x, y, 0f);
    }

    // ── 7. MapController in SCENA DE JOC (SampleScene) + chunk initial ──
    static void SetupSceneMapController(List<GameObject> chunkPrefabs)
    {
        // Daca suntem in alta scena (ex. MainMenu), curatam ce a fost lasat gresit acolo
        Scene active = EditorSceneManager.GetActiveScene();
        if (active.path != GameScenePath)
        {
            CleanupStrayTerrain(active);
            if (active.isDirty) EditorSceneManager.SaveScene(active);
        }

        // Deschidem scena de joc si lucram acolo
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        // Curatam eventuale dubluri inainte de a re-crea
        CleanupStrayTerrain(scene);

        // Dezactivam fundalul vechi (cobblestone) ca sa nu se suprapuna
        var oldBg = GameObject.Find("InfiniteBackground");
        if (oldBg != null) oldBg.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Chunk initial in scena (instanta de prefab -> ChunkTrigger se auto-remapeaza)
        GameObject firstChunk = (GameObject)PrefabUtility.InstantiatePrefab(chunkPrefabs[0]);
        firstChunk.transform.position = Vector3.zero;

        // Map Controller
        GameObject mcGO = new GameObject("Map Controller");
        var mc = mcGO.AddComponent<MapController>();

        mc.terrainChunks = new List<GameObject>(chunkPrefabs);
        mc.player = player;
        mc.checkerRadius = 0.2f;
        mc.terrainMask = 1 << TerrainLayer;
        mc.currentChunk = firstChunk;
        mc.maxOpDist = CHUNK * 3f;          // > dimensiunea tilemap-ului
        mc.optimizerCooldownDur = 1f;
        mc.spawnedChunks = new List<GameObject> { firstChunk };

        EditorUtility.SetDirty(mc);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Sterge obiectele de teren lasate intr-o scena (ex. MainMenu, din rularea anterioara)
    static void CleanupStrayTerrain(Scene scene)
    {
        if (!scene.IsValid()) return;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Map Controller" || root.name.StartsWith("TerrainChunk_"))
                Object.DestroyImmediate(root);
        }
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
