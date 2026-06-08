using CelikenVP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private PlayerStatsSO playerStats;
    [SerializeField] private int maxActiveWeapons = 6;
    [SerializeField] private int maxPassiveObjects = 6;
    public PlayerStatsSO PlayerStats => playerStats;
    public int MaxActiveWeapons => maxActiveWeapons;
    public int MaxPassiveObjects => maxPassiveObjects;

    private void Awake()
    {
        Instance = this;
        if (playerStats != null)
            playerStats.ClearObjects();
    }

    public bool PickItem(ObjectSO item)
    {
        if (playerStats == null || item == null)
            return false;
        if (!playerStats.LevelObject.ContainsKey(item) && playerStats.LevelObject.Count >= maxPassiveObjects)
            return false;
        return playerStats.AddItem(item);
    }

    public bool CanPickItem(ObjectSO item)
    {
        if (playerStats == null || item == null)
            return false;
        return playerStats.LevelObject.ContainsKey(item) || playerStats.LevelObject.Count < maxPassiveObjects;
    }

    public int GetCurrentItemLevel(ObjectSO item)
    {
        if (playerStats == null || item == null)
            return 0;
        return playerStats.GetItemLevel(item);
    }
}
