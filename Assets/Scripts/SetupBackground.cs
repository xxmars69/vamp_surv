using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class SetupBackground : MonoBehaviour
{
    [MenuItem("Joc 2D/Creeaza Fundal Infinit (FINAL AI)")]
    public static void Setup()
    {
        // 1. Facem curatenie (stergem paleta si fundalul vechi)
        GameObject palette = GameObject.Find("SpritePalette");
        if (palette != null) DestroyImmediate(palette);

        string grassPath = "Assets/Sprites/Grass_Custom_Final.png";
        
        TextureImporter importer = AssetImporter.GetAtPath(grassPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; 
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Repeat; 
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        Sprite grassSprite = AssetDatabase.LoadAssetAtPath<Sprite>(grassPath);

        GameObject bgParent = GameObject.Find("InfiniteBackground");
        if (bgParent != null) DestroyImmediate(bgParent);
        bgParent = new GameObject("InfiniteBackground");
        
        float chunkSize = 60f; 

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                GameObject chunk = new GameObject("FinalGrassChunk_" + x + "_" + y);
                chunk.transform.parent = bgParent.transform;
                chunk.transform.position = new Vector3(x * chunkSize, y * chunkSize, 0);
                
                SpriteRenderer sr = chunk.AddComponent<SpriteRenderer>();
                sr.sprite = grassSprite;
                sr.drawMode = SpriteDrawMode.Tiled; 
                sr.size = new Vector2(chunkSize, chunkSize);
                sr.sortingOrder = -100;
            }
        }

        bgParent.AddComponent<InfiniteBackground>().chunkSize = chunkSize;
        Debug.Log("Curatenie facuta! Fundalul FINAL a fost aplicat.");
    }
}
#endif
