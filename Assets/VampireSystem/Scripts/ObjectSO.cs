
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CelikenVP
{
    [CreateAssetMenu(menuName = "Object")]
    public class ObjectSO : ScriptableObject
    {
        public Sprite objectIcon;
        public string objectName;
        public string objectDescription;
        public int objectMaxLevel;
        public int objectRarity;

        public List<Upgrade> upgrades = new List<Upgrade>();
    }
}
