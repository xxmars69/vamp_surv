using CelikenVP;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectRow : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemLevel;
    [SerializeField] private TextMeshProUGUI itemDesc;

    private ObjectSO objectSORef;
    private ItemPicker pickerRef;

    public void Init(ObjectSO item, ItemPicker pickerRef)
    {
        objectSORef = item;
        this.pickerRef = pickerRef;
        itemIcon.sprite = objectSORef.objectIcon;
        itemName.text = objectSORef.objectName;
        itemLevel.text = $"Lv {Player.Instance.GetCurrentItemLevel(objectSORef) + 1}";
        itemDesc.text = objectSORef.objectDescription;
    }

    public void OnClick()
    {
        pickerRef.OnClick(objectSORef);
    }
}

