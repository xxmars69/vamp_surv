using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CelikenVP;

// Bara de inventar (jos-centru): iconitele itemelor luate + nivelul fiecaruia.
public class InventoryUI : MonoBehaviour
{
    private RectTransform row;
    private readonly List<GameObject> slots = new();

    const int SlotSize = 44;
    const int SlotSpacing = 6;

    public void Init(Canvas canvas)
    {
        GameObject rowGO = new GameObject("InventoryRow", typeof(RectTransform));
        rowGO.transform.SetParent(canvas.transform, false);
        row = rowGO.GetComponent<RectTransform>();
        row.anchorMin = new Vector2(0.5f, 0f);
        row.anchorMax = new Vector2(0.5f, 0f);
        row.pivot     = new Vector2(0.5f, 0f);
        row.anchoredPosition = new Vector2(0f, 10f);
        row.sizeDelta = new Vector2(600f, SlotSize);

        var layout = rowGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = SlotSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    public void Refresh()
    {
        if (row == null) return;
        if (Player.Instance == null || Player.Instance.PlayerStats == null) return;

        foreach (var s in slots) Destroy(s);
        slots.Clear();

        foreach (var kv in Player.Instance.PlayerStats.LevelObject)
        {
            ObjectSO item = kv.Key;
            int level = kv.Value;
            if (item == null) continue;

            GameObject slot = new GameObject("Slot_" + item.objectName,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            slot.transform.SetParent(row, false);
            slot.GetComponent<RectTransform>().sizeDelta = new Vector2(SlotSize, SlotSize);
            slot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            // Iconita
            Sprite icon = RuntimeVisualRepair.LoadSpriteRuntime(GameManager.IconPath(item), 32f);
            if (icon == null) icon = item.objectIcon;
            if (icon != null)
            {
                GameObject ic = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                ic.transform.SetParent(slot.transform, false);
                var icRect = ic.GetComponent<RectTransform>();
                icRect.anchorMin = Vector2.zero; icRect.anchorMax = Vector2.one;
                icRect.offsetMin = new Vector2(4f, 4f); icRect.offsetMax = new Vector2(-4f, -4f);
                var icImg = ic.GetComponent<Image>();
                icImg.sprite = icon; icImg.preserveAspect = true;
            }

            // Nivel (jos-dreapta)
            GameObject lv = new GameObject("Lv", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            lv.transform.SetParent(slot.transform, false);
            var lvRect = lv.GetComponent<RectTransform>();
            lvRect.anchorMin = new Vector2(1f, 0f); lvRect.anchorMax = new Vector2(1f, 0f);
            lvRect.pivot = new Vector2(1f, 0f);
            lvRect.sizeDelta = new Vector2(24f, 18f);
            lvRect.anchoredPosition = new Vector2(-2f, 1f);
            var lvText = lv.GetComponent<Text>();
            lvText.text = level.ToString();
            lvText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lvText.fontSize = 16;
            lvText.fontStyle = FontStyle.Bold;
            lvText.color = new Color(1f, 0.9f, 0.3f);
            lvText.alignment = TextAnchor.LowerRight;

            slots.Add(slot);
        }
    }
}
