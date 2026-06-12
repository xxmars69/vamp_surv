using UnityEngine;

// Numar de damage care sare din inamic si dispare (efect Vampire Survivors).
public class DamageNumber : MonoBehaviour
{
    private TextMesh tm;
    private float life = 0.6f;
    private float timer;
    private Vector3 velocity;

    public static void Spawn(Vector3 worldPos, int amount, bool crit = false)
    {
        GameObject go = new GameObject("DmgNumber");
        go.transform.position = worldPos + new Vector3(Random.Range(-0.2f, 0.2f), 0.3f, 0f);

        var dn = go.AddComponent<DamageNumber>();
        var tm = go.AddComponent<TextMesh>();
        tm.text = amount.ToString();
        tm.fontSize = 48;
        tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = crit ? new Color(1f, 0.85f, 0.2f) : Color.white;
        tm.fontStyle = FontStyle.Bold;

        var mr = go.GetComponent<MeshRenderer>();
        mr.sortingOrder = 100;

        dn.tm = tm;
        dn.velocity = new Vector3(Random.Range(-0.5f, 0.5f), 2.2f, 0f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / life;

        transform.position += velocity * Time.deltaTime;
        velocity.y -= 5f * Time.deltaTime; // gravitatie usoara

        if (tm != null)
        {
            Color c = tm.color;
            c.a = Mathf.Clamp01(1f - t);
            tm.color = c;
        }

        if (timer >= life) Destroy(gameObject);
    }
}
