using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    public Transform player;
    public float chunkSize = 20f; // Am marit dimensiunea dalelor
    private Transform[] chunks;

    void Start()
    {
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        chunks = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            chunks[i] = transform.GetChild(i);
    }

    void Update()
    {
        if (player == null) return;

        foreach (Transform chunk in chunks)
        {
            Vector3 pos = chunk.position;
            
            // Verificam daca jucatorul a depasit marginea dalei
            // Daca da, mutam dala in directia opusa pentru a simula infinitul
            float diffX = player.position.x - pos.x;
            float diffY = player.position.y - pos.y;

            if (Mathf.Abs(diffX) > chunkSize * 1.5f)
            {
                pos.x += Mathf.Sign(diffX) * chunkSize * 3f;
            }

            if (Mathf.Abs(diffY) > chunkSize * 1.5f)
            {
                pos.y += Mathf.Sign(diffY) * chunkSize * 3f;
            }
            
            chunk.position = pos;
        }
    }
}
