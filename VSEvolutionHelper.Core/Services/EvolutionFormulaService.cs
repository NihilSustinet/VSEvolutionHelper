using System;
using System.Collections.Generic;
using System.Linq;
using VSItemTooltips.Core.Models;

namespace VSItemTooltips.Core.Services
{
    /// <summary>
    /// Implements evolution formula calculation and validation.
    /// Pure business logic - no Unity, no IL2CPP, fully testable.
    /// </summary>
    public class EvolutionFormulaService : IEvolutionFormulaService
    {
        public EvolutionFormula GetEvolutionFormula(
            WeaponInfo weapon, 
            List<string> ownedPassives, 
            List<string> bannedItems)
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));
            
            if (string.IsNullOrEmpty(weapon.EvolvesInto))
                return null; // Weapon doesn't evolve
            
            var formula = new EvolutionFormula
            {
                BaseWeapon = weapon,
                RequiredPassives = weapon.RequiredPassives ?? new List<PassiveRequirement>()
            };
            
            // Note: EvolvedWeapon needs to be populated by caller (requires game data lookup)
            
            // Check ownership
            ownedPassives = ownedPassives ?? new List<string>();
            bannedItems = bannedItems ?? new List<string>();
            
            var missing = new List<PassiveRequirement>();
            bool hasBanned = false;
            
            foreach (var req in formula.RequiredPassives)
            {
                string reqId = req.WeaponId ?? req.ItemId;
                
                if (bannedItems.Contains(reqId, StringComparer.OrdinalIgnoreCase))
                {
                    hasBanned = true;
                }
                
                if (!ownedPassives.Contains(reqId, StringComparer.OrdinalIgnoreCase))
                {
                    missing.Add(req);
                }
            }
            
            formula.IsComplete = missing.Count == 0 && !hasBanned;
            formula.MissingRequirements = missing;
            formula.HasBannedRequirements = hasBanned;
            
            return formula;
        }

        public EvolutionValidationResult ValidateEvolution(
            WeaponInfo weapon, 
            List<string> ownedPassives)
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));
            
            var result = new EvolutionValidationResult();
            
            if (string.IsNullOrEmpty(weapon.EvolvesInto))
            {
                result.CanEvolve = false;
                result.BlockReason = "Weapon does not evolve";
                return result;
            }
            
            ownedPassives = ownedPassives ?? new List<string>();
            var required = weapon.RequiredPassives ?? new List<PassiveRequirement>();
            
            foreach (var req in required)
            {
                string reqId = req.WeaponId ?? req.ItemId;
                
                if (!ownedPassives.Contains(reqId, StringComparer.OrdinalIgnoreCase))
                {
                    result.MissingPassives.Add(req.Name ?? reqId);
                    
                    if (req.RequiresMaxLevel)
                    {
                        result.RequiresMaxLevel.Add(req.Name ?? reqId);
                    }
                }
            }
            
            if (result.MissingPassives.Count > 0)
            {
                result.CanEvolve = false;
                result.BlockReason = $"Missing {result.MissingPassives.Count} passive(s)";
            }
            else
            {
                result.CanEvolve = true;
            }
            
            return result;
        }

        public List<PassiveRequirement> GetRequiredPassives(WeaponInfo weapon)
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));
            
            return weapon.RequiredPassives ?? new List<PassiveRequirement>();
        }

        public bool IsPrimaryPassive(WeaponInfo weapon, List<EvolutionFormula> allFormulas)
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));
            
            int usageCount = CountPassiveUsages(weapon.Id, allFormulas);
            
            // A weapon is a "primary passive" if:
            // 1. It's used in 2+ evolutions (not counting dual-weapon evolutions)
            // 2. OR it has no evolution of its own but is used as a passive
            
            bool hasOwnEvolution = !string.IsNullOrEmpty(weapon.EvolvesInto);
            
            if (!hasOwnEvolution && usageCount >= 1)
                return true; // No evolution, but used as passive = primary passive
            
            return usageCount >= 2; // Used in multiple evolutions
        }

        public List<EvolutionFormula> GetPassiveUsages(
            string weaponOrItemId, 
            List<EvolutionFormula> allFormulas)
        {
            if (string.IsNullOrEmpty(weaponOrItemId))
                throw new ArgumentNullException(nameof(weaponOrItemId));
            
            allFormulas = allFormulas ?? new List<EvolutionFormula>();
            
            return allFormulas
                .Where(f => f.RequiredPassives != null && f.RequiredPassives.Any(req =>
                    string.Equals(req.WeaponId, weaponOrItemId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(req.ItemId, weaponOrItemId, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public WeaponInfo FindBaseWeapon(
            WeaponInfo evolvedWeapon, 
            List<EvolutionFormula> allFormulas)
        {
            if (evolvedWeapon == null)
                throw new ArgumentNullException(nameof(evolvedWeapon));
            
            allFormulas = allFormulas ?? new List<EvolutionFormula>();
            
            var formula = allFormulas.FirstOrDefault(f =>
                f.EvolvedWeapon != null &&
                string.Equals(f.EvolvedWeapon.Id, evolvedWeapon.Id, StringComparison.OrdinalIgnoreCase));
            
            return formula?.BaseWeapon;
        }

        public int CountPassiveUsages(
            string weaponOrItemId, 
            List<EvolutionFormula> allFormulas)
        {
            if (string.IsNullOrEmpty(weaponOrItemId))
                return 0;
            
            var usages = GetPassiveUsages(weaponOrItemId, allFormulas);
            return usages.Count;
        }
    }
}
