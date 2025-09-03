using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Holder Settings")]
public class WeaponHolderSettings : ScriptableObject
{
    [Header("Weapons")]
    public List<GameObject> weaponPrefabs = new List<GameObject>();
}
