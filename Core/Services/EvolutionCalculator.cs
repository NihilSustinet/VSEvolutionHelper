using System.Collections.Generic;
using System.Linq;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;
using VSItemTooltips.Core.Abstractions;

namespace VSItemTooltips.Core.Services
{
    /// <summary>
    /// Service for calculating evolution formulas and passive relationships.
    /// This class is testable because it only depends on abstractions.
    /// </summary>
    public class EvolutionCalculator
    {
        private readonly IGameDataProvider _gameData;
        private readonly IPlayerStateProvider _playerState;
        private readonly IModLogger _logger;

        public EvolutionCalculator(
            IGameDataProvider gameData,
            IPlayerStateProvider playerState,
            IModLogger logger)
        {
            _gameData = gameData;
            _playerState = playerState;
            _logger = logger;
        }

        /// <summary>
        /// Gets all evolution formulas for a weapon (as base weapon or evolved result).
        /// </summary>
        public List<EvolutionFormula> GetWeaponEvolutions(WeaponType weaponType)
        {
            var formulas = new List<EvolutionFormula>();
            var weaponData = _gameData.GetWeaponData(weaponType);
            if (weaponData == null) return formulas;

            // Check if this weapon evolves into something
            var evoInto = _gameData.GetEvolvedInto(weaponData);
            if (!string.IsNullOrEmpty(evoInto))
            {
                var formula = CreateFormulaForWeapon(weaponType, weaponData);
                if (formula != null)
                {
                    formulas.Add(formula);
                }
            }

            // Check if this weapon is an evolution result
            var evolvedFromFormula = FindEvolvedFromFormula(weaponType);
            if (evolvedFromFormula != null)
            {
                formulas.Add(evolvedFromFormula);
            }

            return formulas;
        }

        /// <summary>
        /// Gets all formulas where this weapon/item is used as a passive requirement.
        /// </summary>
        public List<EvolutionFormula> GetPassiveUsages(WeaponType passiveType)
        {
            var formulas = new List<EvolutionFormula>();
            // TODO: Iterate all weapons and find which ones need this passive
            // This requires the game data provider to expose weapon enumeration
            return formulas;
        }

        /// <summary>
        /// Determines if a weapon is primarily a passive (used in many other evolutions)
        /// vs an active weapon (has its own evolution).
        /// </summary>
        public bool IsPrimaryPassive(WeaponType weaponType)
        {
            var ownEvolution = GetWeaponEvolutions(weaponType);
            var passiveUsages = GetPassiveUsages(weaponType);
            
            // If used as passive in 2+ formulas (excluding dual-weapon partner), it's a passive
            return passiveUsages.Count >= 2;
        }

        private EvolutionFormula CreateFormulaForWeapon(WeaponType baseWeapon, WeaponData weaponData)
        {
            var evoInto = _gameData.GetEvolvedInto(weaponData);
            if (string.IsNullOrEmpty(evoInto)) return null;

            if (!System.Enum.TryParse<WeaponType>(evoInto, out var evolvedType))
                return null;

            var synergy = _gameData.GetEvoSynergy(weaponData);
            var requiresMax = _gameData.GetRequiresMax(weaponData);

            var passives = CollectPassiveRequirements(synergy, requiresMax);

            return new EvolutionFormula
            {
                BaseWeapon = baseWeapon,
                EvolvedWeapon = evolvedType,
                Passives = passives,
                BaseName = _gameData.GetWeaponName(baseWeapon),
                EvolvedName = _gameData.GetWeaponName(evolvedType),
                BaseSprite = _gameData.GetWeaponSprite(baseWeapon),
                EvolvedSprite = _gameData.GetWeaponSprite(evolvedType)
            };
        }

        private EvolutionFormula FindEvolvedFromFormula(WeaponType evolvedType)
        {
            // TODO: Search all weapons to find which one evolves into this type
            // Requires weapon enumeration from game data provider
            return null;
        }

        private List<PassiveRequirement> CollectPassiveRequirements(
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<WeaponType> evoSynergy,
            HashSet<int> requiresMaxTypes)
        {
            var passives = new List<PassiveRequirement>();
            if (evoSynergy == null) return passives;

            for (int i = 0; i < evoSynergy.Length; i++)
            {
                var reqType = evoSynergy[i];
                var passive = new PassiveRequirement
                {
                    RequiresMaxLevel = requiresMaxTypes?.Contains((int)reqType) ?? false
                };

                var reqData = _gameData.GetWeaponData(reqType);
                if (reqData != null)
                {
                    passive.WeaponType = reqType;
                    passive.Sprite = _gameData.GetWeaponSprite(reqType);
                    passive.Owned = _playerState.OwnsWeapon(reqType) || _playerState.OwnsAccessory(reqType);
                }
                else
                {
                    // Try as item
                    int enumValue = (int)reqType;
                    if (System.Enum.IsDefined(typeof(ItemType), enumValue))
                    {
                        var itemType = (ItemType)enumValue;
                        passive.ItemType = itemType;
                        passive.Sprite = _gameData.GetItemSprite(itemType);
                        passive.Owned = _playerState.OwnsItem(itemType);
                    }
                }

                passives.Add(passive);
            }

            return passives;
        }

        /// <summary>
        /// Calculates the position for a popup to avoid screen edges.
        /// Pure business logic - easily testable.
        /// </summary>
        public (float x, float y) CalculatePopupPosition(
            float anchorX, float anchorY,
            float popupWidth, float popupHeight,
            float screenWidth, float screenHeight,
            bool usingController = false)
        {
            float posX, posY;

            if (usingController)
            {
                // Controller mode: popup appears to the left
                posX = anchorX - (popupWidth * 0.5f);
                posY = anchorY + 15f;
            }
            else
            {
                // Mouse mode: popup appears at cursor
                posX = anchorX - 15f;
                posY = anchorY + 40f;
            }

            // Clamp to screen bounds
            float halfWidth = screenWidth / 2;
            float halfHeight = screenHeight / 2;

            // Right edge
            if (posX + popupWidth > halfWidth)
                posX = halfWidth - popupWidth;

            // Left edge
            if (posX < -halfWidth)
                posX = -halfWidth;

            // Top edge
            if (posY > halfHeight)
                posY = halfHeight;

            // Bottom edge
            if (posY - popupHeight < -halfHeight)
                posY = -halfHeight + popupHeight;

            return (posX, posY);
        }
    }

    public class EvolutionFormula
    {
        public WeaponType BaseWeapon { get; set; }
        public WeaponType EvolvedWeapon { get; set; }
        public List<PassiveRequirement> Passives { get; set; }
        public string BaseName { get; set; }
        public string EvolvedName { get; set; }
        public UnityEngine.Sprite BaseSprite { get; set; }
        public UnityEngine.Sprite EvolvedSprite { get; set; }
    }

    public class PassiveRequirement
    {
        public WeaponType? WeaponType { get; set; }
        public ItemType? ItemType { get; set; }
        public UnityEngine.Sprite Sprite { get; set; }
        public bool Owned { get; set; }
        public bool RequiresMaxLevel { get; set; }
    }
}
