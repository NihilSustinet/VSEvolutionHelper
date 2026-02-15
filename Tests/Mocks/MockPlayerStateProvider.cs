using System.Collections.Generic;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;
using VSItemTooltips.Core.Abstractions;

namespace VSEvolutionHelper.Tests.Mocks
{
    public class MockPlayerStateProvider : IPlayerStateProvider
    {
        private readonly HashSet<WeaponType> _ownedWeapons = new HashSet<WeaponType>();
        private readonly HashSet<ItemType> _ownedItems = new HashSet<ItemType>();
        private readonly HashSet<WeaponType> _bannedWeapons = new HashSet<WeaponType>();
        private readonly HashSet<ItemType> _bannedItems = new HashSet<ItemType>();

        public void SetOwnsWeapon(WeaponType type, bool owned = true)
        {
            if (owned) _ownedWeapons.Add(type);
            else _ownedWeapons.Remove(type);
        }

        public void SetOwnsItem(ItemType type, bool owned = true)
        {
            if (owned) _ownedItems.Add(type);
            else _ownedItems.Remove(type);
        }

        public void SetBannedWeapon(WeaponType type, bool banned = true)
        {
            if (banned) _bannedWeapons.Add(type);
            else _bannedWeapons.Remove(type);
        }

        public void SetBannedItem(ItemType type, bool banned = true)
        {
            if (banned) _bannedItems.Add(type);
            else _bannedItems.Remove(type);
        }

        public bool OwnsWeapon(WeaponType weaponType) => _ownedWeapons.Contains(weaponType);
        public bool OwnsItem(ItemType itemType) => _ownedItems.Contains(itemType);
        public bool OwnsAccessory(WeaponType weaponType) => _ownedWeapons.Contains(weaponType);
        public bool IsWeaponBanned(WeaponType weaponType) => _bannedWeapons.Contains(weaponType);
        public bool IsItemBanned(ItemType itemType) => _bannedItems.Contains(itemType);
        public bool IsArcanaActive(object arcanaType) => false;
        public List<object> GetActiveArcanas() => new List<object>();
    }
}
