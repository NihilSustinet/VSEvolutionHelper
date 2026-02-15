using System.Collections.Generic;
using VSItemTooltips.Core.Models;

namespace VSItemTooltips.Core.Services
{
    /// <summary>
    /// Service for calculating weapon evolution formulas and validating requirements.
    /// Pure business logic with no game/Unity dependencies - fully testable.
    /// </summary>
    public interface IEvolutionFormulaService
    {
        /// <summary>
        /// Gets the complete evolution formula for a weapon.
        /// Returns null if weapon doesn't evolve.
        /// </summary>
        EvolutionFormula GetEvolutionFormula(WeaponInfo weapon, List<string> ownedPassives, List<string> bannedItems);
        
        /// <summary>
        /// Validates if a weapon can evolve with current owned passives.
        /// </summary>
        EvolutionValidationResult ValidateEvolution(WeaponInfo weapon, List<string> ownedPassives);
        
        /// <summary>
        /// Gets all passives required for a weapon's evolution.
        /// </summary>
        List<PassiveRequirement> GetRequiredPassives(WeaponInfo weapon);
        
        /// <summary>
        /// Determines if a weapon is primarily used as a passive (vs. active weapon).
        /// A weapon is a "primary passive" if it's used in 2+ other evolutions.
        /// </summary>
        bool IsPrimaryPassive(WeaponInfo weapon, List<EvolutionFormula> allFormulas);
        
        /// <summary>
        /// Gets all evolution formulas that use this weapon/item as a passive.
        /// </summary>
        List<EvolutionFormula> GetPassiveUsages(string weaponOrItemId, List<EvolutionFormula> allFormulas);
        
        /// <summary>
        /// Finds the base weapon that evolves into the given evolved weapon.
        /// Returns null if this is not an evolved weapon.
        /// </summary>
        WeaponInfo FindBaseWeapon(WeaponInfo evolvedWeapon, List<EvolutionFormula> allFormulas);
        
        /// <summary>
        /// Counts how many times a passive is used across all evolutions.
        /// </summary>
        int CountPassiveUsages(string weaponOrItemId, List<EvolutionFormula> allFormulas);
    }
}
