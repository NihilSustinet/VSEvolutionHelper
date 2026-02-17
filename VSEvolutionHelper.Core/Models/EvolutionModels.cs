using System.Collections.Generic;

namespace VSItemTooltips.Core.Models
{
    /// <summary>
    /// Pure C# representation of a weapon (no IL2CPP dependencies).
    /// Used for evolution calculations and testing.
    /// </summary>
    public class WeaponInfo
    {
        /// <summary>Weapon identifier (e.g., "WHIP")</summary>
        public string Id { get; set; }
        
        /// <summary>Display name (e.g., "Whip")</summary>
        public string Name { get; set; }
        
        /// <summary>Weapon description</summary>
        public string Description { get; set; }
        
        /// <summary>What this weapon evolves into (weapon ID), null if doesn't evolve</summary>
        public string EvolvesInto { get; set; }
        
        /// <summary>Required passive weapons/items for evolution</summary>
        public List<PassiveRequirement> RequiredPassives { get; set; } = new List<PassiveRequirement>();
        
        /// <summary>Is this weapon primarily used as a passive in other evolutions?</summary>
        public bool IsPrimaryPassive { get; set; }
        
        /// <summary>Is this an evolved weapon (result of evolution)?</summary>
        public bool IsEvolved { get; set; }
    }

    /// <summary>
    /// Pure C# representation of an item/power-up.
    /// </summary>
    public class ItemInfo
    {
        /// <summary>Item identifier (e.g., "SPINACH")</summary>
        public string Id { get; set; }
        
        /// <summary>Display name (e.g., "Spinach")</summary>
        public string Name { get; set; }
        
        /// <summary>Item description</summary>
        public string Description { get; set; }
        
        /// <summary>List of weapon evolutions this item is used in</summary>
        public List<string> UsedInEvolutions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents a required passive for weapon evolution.
    /// </summary>
    public class PassiveRequirement
    {
        /// <summary>Weapon ID if passive is a weapon</summary>
        public string WeaponId { get; set; }
        
        /// <summary>Item ID if passive is an item</summary>
        public string ItemId { get; set; }
        
        /// <summary>Display name of the passive</summary>
        public string Name { get; set; }
        
        /// <summary>Does this passive require max level?</summary>
        public bool RequiresMaxLevel { get; set; }
        
        /// <summary>Is this passive type (weapon or item)?</summary>
        public PassiveType Type { get; set; }
    }

    public enum PassiveType
    {
        Weapon,
        Item
    }
}
