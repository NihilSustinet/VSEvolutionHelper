using System.Collections.Generic;
using System.Linq;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;
using UnityEngine;
using VSItemTooltips.Core.Abstractions;

namespace VSEvolutionHelper.Tests.Mocks
{
    /// <summary>
    /// Mock game data provider for testing.
    /// Allows setting up test scenarios without game/IL2CPP dependencies.
    /// </summary>
    public class MockGameDataProvider : IGameDataProvider
    {
        private readonly Dictionary<WeaponType, MockWeaponData> _weapons = new Dictionary<WeaponType, MockWeaponData>();
        private readonly Dictionary<ItemType, MockItemData> _items = new Dictionary<ItemType, MockItemData>();

        public void AddWeapon(WeaponType type, string name, string desc, string evolvesInto = null, params WeaponType[] synergy)
        {
            _weapons[type] = new MockWeaponData
            {
                Type = type,
                Name = name,
                Description = desc,
                EvolvesInto = evolvesInto,
                Synergy = synergy
            };
        }

        public void AddItem(ItemType type, string name, string desc)
        {
            _items[type] = new MockItemData
            {
                Type = type,
                Name = name,
                Description = desc
            };
        }

        public WeaponData GetWeaponData(WeaponType type)
        {
            // Can't actually return WeaponData (IL2CPP type) in unit tests
            // This is a limitation - we'd need to mock this differently
            // or make EvolutionCalculator work with DTOs instead
            return null;
        }

        public bool WeaponExists(WeaponType type) => _weapons.ContainsKey(type);

        public string GetWeaponName(WeaponType type) =>
            _weapons.TryGetValue(type, out var data) ? data.Name : type.ToString();

        public string GetWeaponDescription(WeaponType type) =>
            _weapons.TryGetValue(type, out var data) ? data.Description : "";

        public Sprite GetWeaponSprite(WeaponType type) => null; // Can't create real sprites in tests

        public object GetPowerUpData(ItemType type) => _items.GetValueOrDefault(type);

        public bool ItemExists(ItemType type) => _items.ContainsKey(type);

        public string GetItemName(ItemType type) =>
            _items.TryGetValue(type, out var data) ? data.Name : type.ToString();

        public string GetItemDescription(ItemType type) =>
            _items.TryGetValue(type, out var data) ? data.Description : "";

        public Sprite GetItemSprite(ItemType type) => null;

        public object GetArcanaData(object arcanaType) => null;
        public List<object> GetAllActiveArcanaTypes() => new List<object>();
        public string GetArcanaName(object arcanaData) => "";
        public string GetArcanaDescription(object arcanaData) => "";
        public Sprite GetArcanaSprite(object arcanaData) => null;

        public string GetEvolvedInto(WeaponData weaponData) => null;
        
        public Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<WeaponType> GetEvoSynergy(WeaponData weaponData) => null;
        
        public HashSet<int> GetRequiresMax(WeaponData weaponData) => null;

        private class MockWeaponData
        {
            public WeaponType Type { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string EvolvesInto { get; set; }
            public WeaponType[] Synergy { get; set; }
        }

        private class MockItemData
        {
            public ItemType Type { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
