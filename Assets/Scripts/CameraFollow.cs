using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float smoothSpeed = 5f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    // Pixel per Unit al tilemap-ului - trebuie sa fie identic cu sprite-urile
    // (terrain are PPU 32). Snap-uim camera la grila de pixeli ca sa nu apara
    // linii intre tile-uri cand camera e la pozitii fractionare.
    const float PPU = 32f;

    void LateUpdate()
    {
        if (player == null) return;

        // Urmarire directa (fara Lerp) + snap la pixel grid.
        // Lerp-ul producea pozitii fractionare la fiecare frame ⇒ cusaturi galbene
        // intre tile-uri si lag perceput la miscare. Fara Lerp e instant si crisp.
        Vector3 p = player.position + offset;
        p.x = Mathf.Round(p.x * PPU) / PPU;
        p.y = Mathf.Round(p.y * PPU) / PPU;
        transform.position = p;
    }
}
