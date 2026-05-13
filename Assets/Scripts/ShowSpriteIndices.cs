using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class ShowSpriteIndices : MonoBehaviour
{
    [MenuItem("Joc 2D/Afiseaza Paleta de Sprite-uri")]
    public static void ShowPalette()
    {
        string tilePath = "Assets/Sprites/forest_tile.png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(tilePath);
        
        GameObject paletteParent = GameObject.Find("SpritePalette");
        if (paletteParent != null) DestroyImmediate(paletteParent);
        paletteParent = new GameObject("SpritePalette");

        int index = 0;
        int col = 0;
        int row = 0;
        int maxCols = 16;

        foreach (var asset in assets)
        {
            if (asset is Sprite s)
            {
                GameObject obj = new GameObject("Sprite_" + index);
                obj.transform.parent = paletteParent.transform;
                obj.transform.position = new Vector3(col * 2, -row * 2, 0);

                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = s;

                // Adaugam numarul sub sprite
                GameObject textObj = new GameObject("IndexText");
                textObj.transform.parent = obj.transform;
                textObj.transform.localPosition = new Vector3(0, -1f, 0);
                TextMesh tm = textObj.AddComponent<TextMesh>();
                tm.text = index.ToString();
                tm.characterSize = 0.2f;
                tm.anchor = TextAnchor.UpperCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = Color.yellow;

                index++;
                col++;
                if (col >= maxCols) { col = 0; row++; }
            }
        }
        
        Selection.activeGameObject = paletteParent;
        SceneView.lastActiveSceneView.FrameSelected();
        Debug.Log("Paleta a fost creata! Uita-te in centrul scenei la numere.");
    }
}
#endif
