using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

#if UNITY_EDITOR
public class SetupEnemySprites : MonoBehaviour
{
    [MenuItem("Joc 2D/Pune Skinul Bat pe Inamici")]
    public static void Setup()
    {
        string sheetPath = "Assets/Sprites/Enemies/BatStandard_Sheet.png";
        
        // 1. Configuram Sprite Sheet-ul (Slicing)
        TextureImporter importer = AssetImporter.GetAtPath(sheetPath) as TextureImporter;
        if (importer == null) { Debug.LogError("Nu am gasit sheet-ul la: " + sheetPath); return; }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // Definim cadrele (Flying sunt de obicei primele 4 cadre)
        // Presupunem cadre de 32x32 sau 16x16. Bat-ul pare mic, incercam 32x32.
        int frameWidth = 32;
        int frameHeight = 32;
        
        // Incarcam textura pentru a vedea dimensiunile
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
        int columns = tex.width / frameWidth;
        int rows = tex.height / frameHeight;

        List<SpriteMetaData> metas = new List<SpriteMetaData>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                SpriteMetaData meta = new SpriteMetaData();
                meta.rect = new Rect(c * frameWidth, (rows - 1 - r) * frameHeight, frameWidth, frameHeight);
                meta.name = "Bat_" + r + "_" + c;
                metas.Add(meta);
            }
        }
        importer.spritesheet = metas.ToArray();
        importer.SaveAndReimport();

        // 2. Gasim Sprite-urile rezultate
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
        List<Sprite> flyingSprites = new List<Sprite>();
        foreach (var asset in assets)
        {
            if (asset is Sprite) flyingSprites.Add(asset as Sprite);
        }

        // 3. Cream Animatia
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Enemies"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
            AssetDatabase.CreateFolder("Assets/Animations", "Enemies");
        }

        AnimationClip flyClip = new AnimationClip();
        flyClip.frameRate = 10;
        EditorCurveBinding binding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        
        // Folosim primele 4 cadre pentru zbor
        int animFrames = Mathf.Min(4, flyingSprites.Count);
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[animFrames];
        for (int i = 0; i < animFrames; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe { time = i / 10f, value = flyingSprites[i] };
        }
        AnimationUtility.SetObjectReferenceCurve(flyClip, binding, keyframes);
        
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(flyClip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(flyClip, settings);

        AssetDatabase.CreateAsset(flyClip, "Assets/Animations/Enemies/Bat_Fly.anim");

        // 4. Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Animations/Enemies/BatAnimator.controller");
        controller.layers[0].stateMachine.AddState("Fly").motion = flyClip;

        // 5. Aplicam pe Prefabul Enemy
        string prefabPath = "Assets/Prefabs/Enemy.prefab";
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (enemyPrefab != null)
        {
            SpriteRenderer sr = enemyPrefab.GetComponent<SpriteRenderer>();
            sr.sprite = flyingSprites[0];
            sr.color = Color.white; // Scoatem culoarea rosie

            Animator anim = enemyPrefab.GetComponent<Animator>();
            if (anim == null) anim = enemyPrefab.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            // Scale liliacul (sa fie vizibil)
            enemyPrefab.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            EditorUtility.SetDirty(enemyPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("GATA! Inamicii sunt acum lilieci animati!");
        }
        else
        {
            Debug.LogError("Nu am gasit prefabul Enemy la: " + prefabPath);
        }
    }
}
#endif
