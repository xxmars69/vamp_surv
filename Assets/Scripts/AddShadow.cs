using UnityEngine;

public class AddShadow : MonoBehaviour
{
    void Start()
    {
        // Cream un obiect nou pentru umbra
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.parent = transform;
        shadow.transform.localPosition = new Vector3(0, -0.4f, 0); // Putin sub picioare
        
        SpriteRenderer sr = shadow.AddComponent<SpriteRenderer>();
        // Folosim un sprite de tip cerc (il gasim in Unity by default sau il simulam)
        // Daca nu ai un sprite de cerc, folosim o tufa setata pe negru transparent
        sr.color = new Color(0, 0, 0, 0.3f); // Negru transparent
        sr.sortingOrder = -10; // In spatele personajului, dar peste iarba
        
        // Poti pune aici orice sprite rotund ai (ex: o minge sau chiar un tile de pamant rotund)
        // Incercam sa gasim un sprite de baza in Unity
        sr.sprite = Resources.GetBuiltinResource<Sprite>("ShadowBlob.psd");
        if (sr.sprite == null) {
            // Daca nu gasim, punem un scale mic pe un tile oarecare
            shadow.transform.localScale = new Vector3(0.8f, 0.3f, 1f);
        } else {
            shadow.transform.localScale = new Vector3(1f, 0.4f, 1f);
        }
    }
}
