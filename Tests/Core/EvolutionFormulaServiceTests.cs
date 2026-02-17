using System.Collections.Generic;
using Xunit;
using VSItemTooltips.Core.Models;
using VSItemTooltips.Core.Services;

namespace VSEvolutionHelper.Tests.Core
{
    /// <summary>
    /// Tests for evolution formula calculation and validation.
    /// Based on real Vampire Survivors evolution mechanics.
    /// </summary>
    public class EvolutionFormulaServiceTests
    {
        private readonly EvolutionFormulaService _service;

        public EvolutionFormulaServiceTests()
        {
            _service = new EvolutionFormulaService();
        }

        #region GetEvolutionFormula Tests

        [Fact]
        public void GetEvolutionFormula_WeaponDoesNotEvolve_ReturnsNull()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "GARLIC",
                Name = "Garlic",
                EvolvesInto = null // Doesn't evolve
            };

            // Act
            var result = _service.GetEvolutionFormula(weapon, new List<string>(), new List<string>());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetEvolutionFormula_SimpleEvolution_ReturnsFormula()
        {
            // Arrange - Whip + Hollow Heart = Bloody Tear
            var whip = new WeaponInfo
            {
                Id = "WHIP",
                Name = "Whip",
                EvolvesInto = "BLOODY_TEAR",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement
                    {
                        ItemId = "HOLLOW_HEART",
                        Name = "Hollow Heart",
                        Type = PassiveType.Item,
                        RequiresMaxLevel = false
                    }
                }
            };

            var owned = new List<string> { "HOLLOW_HEART" };

            // Act
            var result = _service.GetEvolutionFormula(whip, owned, new List<string>());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WHIP", result.BaseWeapon.Id);
            Assert.Single(result.RequiredPassives);
            Assert.True(result.IsComplete);
            Assert.Empty(result.MissingRequirements);
            Assert.False(result.HasBannedRequirements);
        }

        [Fact]
        public void GetEvolutionFormula_MissingPassive_IsNotComplete()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "MAGIC_WAND",
                Name = "Magic Wand",
                EvolvesInto = "HOLY_WAND",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "EMPTY_TOME", Name = "Empty Tome", Type = PassiveType.Item }
                }
            };

            var owned = new List<string>(); // Don't own the tome

            // Act
            var result = _service.GetEvolutionFormula(weapon, owned, new List<string>());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsComplete);
            Assert.Single(result.MissingRequirements);
            Assert.Equal("Empty Tome", result.MissingRequirements[0].Name);
        }

        [Fact]
        public void GetEvolutionFormula_BannedPassive_HasBannedRequirements()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "KNIFE",
                Name = "Knife",
                EvolvesInto = "THOUSAND_EDGE",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "BRACER", Name = "Bracer", Type = PassiveType.Item }
                }
            };

            var owned = new List<string> { "BRACER" };
            var banned = new List<string> { "BRACER" }; // Item is banned

            // Act
            var result = _service.GetEvolutionFormula(weapon, owned, banned);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasBannedRequirements);
            Assert.False(result.IsComplete); // Can't be complete if banned
        }

        [Fact]
        public void GetEvolutionFormula_MultiplePassives_TracksAllRequirements()
        {
            // Arrange - Weapon needs 2 passives
            var weapon = new WeaponInfo
            {
                Id = "TEST_WEAPON",
                Name = "Test Weapon",
                EvolvesInto = "EVOLVED_WEAPON",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "ITEM1", Name = "Item 1", Type = PassiveType.Item },
                    new PassiveRequirement { WeaponId = "WEAPON1", Name = "Weapon 1", Type = PassiveType.Weapon, RequiresMaxLevel = true }
                }
            };

            var owned = new List<string> { "ITEM1" }; // Only own one

            // Act
            var result = _service.GetEvolutionFormula(weapon, owned, new List<string>());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsComplete);
            Assert.Single(result.MissingRequirements);
            Assert.Equal("Weapon 1", result.MissingRequirements[0].Name);
            Assert.True(result.MissingRequirements[0].RequiresMaxLevel);
        }

        #endregion

        #region ValidateEvolution Tests

        [Fact]
        public void ValidateEvolution_AllRequirementsMet_CanEvolve()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "AXE",
                Name = "Axe",
                EvolvesInto = "DEATH_SPIRAL",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "CANDELABRADOR", Name = "Candelabrador", Type = PassiveType.Item }
                }
            };

            var owned = new List<string> { "CANDELABRADOR" };

            // Act
            var result = _service.ValidateEvolution(weapon, owned);

            // Assert
            Assert.True(result.CanEvolve);
            Assert.Empty(result.MissingPassives);
            Assert.Empty(result.RequiresMaxLevel);
            Assert.Null(result.BlockReason);
        }

        [Fact]
        public void ValidateEvolution_MissingPassive_CannotEvolve()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "CROSS",
                Name = "Cross",
                EvolvesInto = "HEAVEN_SWORD",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "CLOVER", Name = "Clover", Type = PassiveType.Item }
                }
            };

            var owned = new List<string>(); // Don't own clover

            // Act
            var result = _service.ValidateEvolution(weapon, owned);

            // Assert
            Assert.False(result.CanEvolve);
            Assert.Single(result.MissingPassives);
            Assert.Contains("Clover", result.MissingPassives);
            Assert.Equal("Missing 1 passive(s)", result.BlockReason);
        }

        [Fact]
        public void ValidateEvolution_WeaponDoesNotEvolve_CannotEvolve()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "LAUREL",
                Name = "Laurel",
                EvolvesInto = null // Doesn't evolve
            };

            // Act
            var result = _service.ValidateEvolution(weapon, new List<string>());

            // Assert
            Assert.False(result.CanEvolve);
            Assert.Equal("Weapon does not evolve", result.BlockReason);
        }

        [Fact]
        public void ValidateEvolution_MaxLevelRequirement_TrackedSeparately()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "SONG",
                Name = "Song",
                EvolvesInto = "MANNAJJA",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement
                    {
                        WeaponId = "SKULL",
                        Name = "Skull O'Maniac",
                        Type = PassiveType.Weapon,
                        RequiresMaxLevel = true // Needs max level
                    }
                }
            };

            var owned = new List<string>(); // Don't own skull

            // Act
            var result = _service.ValidateEvolution(weapon, owned);

            // Assert
            Assert.False(result.CanEvolve);
            Assert.Single(result.MissingPassives);
            Assert.Single(result.RequiresMaxLevel);
            Assert.Contains("Skull O'Maniac", result.RequiresMaxLevel);
        }

        #endregion

        #region GetRequiredPassives Tests

        [Fact]
        public void GetRequiredPassives_WeaponWithPassives_ReturnsPassives()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "KING_BIBLE",
                Name = "King Bible",
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "SPELLBINDER", Name = "Spellbinder", Type = PassiveType.Item }
                }
            };

            // Act
            var result = _service.GetRequiredPassives(weapon);

            // Assert
            Assert.Single(result);
            Assert.Equal("Spellbinder", result[0].Name);
        }

        [Fact]
        public void GetRequiredPassives_WeaponWithNoPassives_ReturnsEmpty()
        {
            // Arrange
            var weapon = new WeaponInfo
            {
                Id = "GARLIC",
                Name = "Garlic",
                RequiredPassives = null
            };

            // Act
            var result = _service.GetRequiredPassives(weapon);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region IsPrimaryPassive Tests

        [Fact]
        public void IsPrimaryPassive_UsedInTwoEvolutions_ReturnsTrue()
        {
            // Arrange - Spinach is used in multiple evolutions
            var spinach = new WeaponInfo { Id = "SPINACH", Name = "Spinach", EvolvesInto = null };
            
            var formulas = new List<EvolutionFormula>
            {
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { ItemId = "SPINACH", Type = PassiveType.Item }
                    }
                },
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { ItemId = "SPINACH", Type = PassiveType.Item }
                    }
                }
            };

            // Act
            var result = _service.IsPrimaryPassive(spinach, formulas);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPrimaryPassive_NoOwnEvolutionButUsedAsPassive_ReturnsTrue()
        {
            // Arrange - Item has no evolution but is used as passive
            var item = new WeaponInfo { Id = "ARMOR", Name = "Armor", EvolvesInto = null };
            
            var formulas = new List<EvolutionFormula>
            {
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { ItemId = "ARMOR", Type = PassiveType.Item }
                    }
                }
            };

            // Act
            var result = _service.IsPrimaryPassive(item, formulas);

            // Assert
            Assert.True(result); // No evolution + used as passive = primary passive
        }

        [Fact]
        public void IsPrimaryPassive_HasEvolutionAndUsedOnce_ReturnsFalse()
        {
            // Arrange - Weapon has its own evolution and only used in one other
            var weapon = new WeaponInfo { Id = "WHIP", Name = "Whip", EvolvesInto = "BLOODY_TEAR" };
            
            var formulas = new List<EvolutionFormula>
            {
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { WeaponId = "WHIP", Type = PassiveType.Weapon }
                    }
                }
            };

            // Act
            var result = _service.IsPrimaryPassive(weapon, formulas);

            // Assert
            Assert.False(result); // Has evolution + used only once = not primary passive
        }

        #endregion

        #region CountPassiveUsages Tests

        [Fact]
        public void CountPassiveUsages_UsedInMultiple_ReturnsCount()
        {
            // Arrange
            var formulas = new List<EvolutionFormula>
            {
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { ItemId = "BRACER" }
                    }
                },
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { ItemId = "BRACER" }
                    }
                },
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { ItemId = "SPINACH" } // Different item
                    }
                }
            };

            // Act
            var result = _service.CountPassiveUsages("BRACER", formulas);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void CountPassiveUsages_NotUsed_ReturnsZero()
        {
            // Arrange
            var formulas = new List<EvolutionFormula>
            {
                new EvolutionFormula
                {
                    RequiredPassives = new List<PassiveRequirement>
                    {
                        new PassiveRequirement { ItemId = "SPINACH" }
                    }
                }
            };

            // Act
            var result = _service.CountPassiveUsages("CLOVER", formulas);

            // Assert
            Assert.Equal(0, result);
        }

        #endregion

        #region FindBaseWeapon Tests

        [Fact]
        public void FindBaseWeapon_EvolvedWeaponExists_ReturnsBase()
        {
            // Arrange
            var whip = new WeaponInfo { Id = "WHIP", Name = "Whip" };
            var bloodyTear = new WeaponInfo { Id = "BLOODY_TEAR", Name = "Bloody Tear" };
            
            var formulas = new List<EvolutionFormula>
            {
                new EvolutionFormula
                {
                    BaseWeapon = whip,
                    EvolvedWeapon = bloodyTear
                }
            };

            // Act
            var result = _service.FindBaseWeapon(bloodyTear, formulas);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WHIP", result.Id);
        }

        [Fact]
        public void FindBaseWeapon_NotEvolvedWeapon_ReturnsNull()
        {
            // Arrange
            var garlic = new WeaponInfo { Id = "GARLIC", Name = "Garlic" };
            var formulas = new List<EvolutionFormula>();

            // Act
            var result = _service.FindBaseWeapon(garlic, formulas);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetPassiveUsages Tests

        [Fact]
        public void GetPassiveUsages_UsedInMultiple_ReturnsAllFormulas()
        {
            // Arrange
            var formula1 = new EvolutionFormula
            {
                BaseWeapon = new WeaponInfo { Id = "WEAPON1" },
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "SPINACH" }
                }
            };
            
            var formula2 = new EvolutionFormula
            {
                BaseWeapon = new WeaponInfo { Id = "WEAPON2" },
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "SPINACH" }
                }
            };
            
            var formulas = new List<EvolutionFormula> { formula1, formula2 };

            // Act
            var result = _service.GetPassiveUsages("SPINACH", formulas);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(formula1, result);
            Assert.Contains(formula2, result);
        }

        [Fact]
        public void GetPassiveUsages_CaseInsensitive_FindsMatches()
        {
            // Arrange
            var formula = new EvolutionFormula
            {
                RequiredPassives = new List<PassiveRequirement>
                {
                    new PassiveRequirement { ItemId = "SPINACH" }
                }
            };
            
            var formulas = new List<EvolutionFormula> { formula };

            // Act
            var result = _service.GetPassiveUsages("spinach", formulas); // lowercase

            // Assert
            Assert.Single(result);
        }

        #endregion
    }
}
