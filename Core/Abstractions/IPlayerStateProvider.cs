using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;

namespace VSItemTooltips.Core.Abstractions
{
    /// <summary>
    /// Abstraction for checking player's current run state (ownership, bans, active arcanas).
    /// Allows mocking player state in tests.
    /// </summary>
    public interface IPlayerStateProvider
    {
        bool OwnsWeapon(WeaponType weaponType);
        bool OwnsItem(ItemType itemType);
        bool OwnsAccessory(WeaponType weaponType);
        
        bool IsWeaponBanned(WeaponType weaponType);
        bool IsItemBanned(ItemType itemType);
        
        bool IsArcanaActive(object arcanaType);
        System.Collections.Generic.List<object> GetActiveArcanas();
    }
}
