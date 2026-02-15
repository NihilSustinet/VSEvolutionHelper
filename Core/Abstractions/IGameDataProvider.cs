using Il2CppVampireSurvivors.Data.Weapons;
using Il2CppVampireSurvivors.Data;
using UnityEngine;

namespace VSItemTooltips.Core.Abstractions
{
    /// <summary>
    /// Abstraction for accessing game data (weapons, items, arcanas).
    /// Allows mocking game data in tests without IL2CPP dependencies.
    /// </summary>
    public interface IGameDataProvider
    {
        // Weapon data
        WeaponData GetWeaponData(WeaponType type);
        bool WeaponExists(WeaponType type);
        string GetWeaponName(WeaponType type);
        string GetWeaponDescription(WeaponType type);
        Sprite GetWeaponSprite(WeaponType type);
        
        // Item/PowerUp data
        object GetPowerUpData(ItemType type);
        bool ItemExists(ItemType type);
        string GetItemName(ItemType type);
        string GetItemDescription(ItemType type);
        Sprite GetItemSprite(ItemType type);
        
        // Arcana data
        object GetArcanaData(object arcanaType);
        System.Collections.Generic.List<object> GetAllActiveArcanaTypes();
        string GetArcanaName(object arcanaData);
        string GetArcanaDescription(object arcanaData);
        Sprite GetArcanaSprite(object arcanaData);
        
        // Evolution data
        string GetEvolvedInto(WeaponData weaponData);
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<WeaponType> GetEvoSynergy(WeaponData weaponData);
        System.Collections.Generic.HashSet<int> GetRequiresMax(WeaponData weaponData);
    }
}
