using CelikenVP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private PlayerStatsSO playerStats;

    private void Awake()
    {
        Instance = this;
        playerStats.ClearObjects();
    }

    public bool PickItem(ObjectSO item)
    {
        return playerStats.AddItem(item);
    }

    public int GetCurrentItemLevel(ObjectSO item)
    {
        return playerStats.GetItemLevel(item);
    }
}

