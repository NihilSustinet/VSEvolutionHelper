using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;
using MelonLoader;

namespace VSItemTooltips
{
    /// <summary>
    /// Handles rendering of evolution formula UI sections in item/weapon tooltips.
    /// Encapsulates all logic for creating evolution icons, passive requirements, arrows, etc.
    /// Uses DataAccessHelper for sprites/data and EvolutionFormulaCache for fast lookups.
    /// </summary>
    public static class EvolutionUIBuilder
    {
        #region Constants

        private static readonly float IconSize = 38f;
        private static readonly float Padding = 12f;
        private static readonly float Spacing = 8f;

        #endregion

        #region Helper Structures

        /// <summary>
        /// Represents a single passive requirement in an evolution formula.
        /// </summary>
        public struct PassiveRequirement
        {
            public WeaponType? WeaponType;
            public ItemType? ItemType;
            public UnityEngine.Sprite Sprite;
            public bool Owned;
            public bool RequiresMaxLevel;
        }

        /// <summary>
        /// Result of rendering a UI section, tracking how much vertical space was used.
        /// </summary>
        public struct RenderResult
        {
            public float YOffsetAfter; // Y position after rendering
            public int ElementsCreated; // Number of UI elements created
        }

        #endregion

        #region Main Evolution Sections

        /// <summary>
        /// Adds a weapon's evolution section showing base weapon + passives → evolved weapon.
        /// Returns the Y offset after rendering.
        /// </summary>
        public static float AddWeaponEvolutionSection(
            UnityEngine.Transform parent,
            Il2CppTMPro.TMP_FontAsset font,
            WeaponType weaponType,
            float yOffset,
            float maxWidth)
        {
            // Use cache for fast lookup
            if (GameDataCache.EvolutionCache == null)
                return yOffset; // Cache not available

            // Count how many OTHER weapons use this one as a passive ingredient
            int passiveUseCount = GameDataCache.EvolutionCache.CountPassiveUsages(weaponType);

            // If this weapon is used as a passive in multiple recipes, show comprehensive view
            if (passiveUseCount >= 2)
            {
                yOffset = AddPassiveEvolutionSection(parent, font, weaponType, yOffset, maxWidth);
                yOffset = AddEvolvedFromSection(parent, font, weaponType, yOffset, maxWidth);
                return yOffset;
            }

            // Check if this weapon is the result of another weapon's evolution
            yOffset = AddEvolvedFromSection(parent, font, weaponType, yOffset, maxWidth);

            // Get this weapon's own evolution from cache - O(1) lookup
            var cachedFormula = GameDataCache.EvolutionCache.GetForWeapon(weaponType);
            if (cachedFormula == null) return yOffset; // No evolution

            // Parse evolved weapon type
            if (!System.Enum.TryParse<WeaponType>(cachedFormula.EvolvedWeaponId, out var evoType))
                return yOffset;

            // Build UI passives list with sprites and ownership
            var passiveRequirements = BuildPassiveRequirements(cachedFormula);

            // Get sprites
            var evoSprite = DataAccessHelper.GetSpriteForWeapon(evoType);
            var weaponSprite = DataAccessHelper.GetSpriteForWeapon(weaponType);
            bool ownsWeapon = DataAccessHelper.PlayerOwnsWeapon(weaponType);

            // Render section
            return RenderEvolutionFormula(
                parent,
                font,
                weaponType,
                passiveRequirements,
                evoType,
                weaponSprite,
                evoSprite,
                yOffset,
                maxWidth,
                "Evolutions: (click for details)"
            );
        }

        /// <summary>
        /// For evolved weapons, shows what base weapon + passives created this evolution.
        /// Uses EvolutionFormulaCache for O(1) lookup.
        /// </summary>
        public static float AddEvolvedFromSection(
            UnityEngine.Transform parent,
            Il2CppTMPro.TMP_FontAsset font,
            WeaponType evolvedType,
            float yOffset,
            float maxWidth)
        {
            if (GameDataCache.EvolutionCache == null) return yOffset;

            var cachedFormula = GameDataCache.EvolutionCache.GetForEvolvedWeapon(evolvedType);
            if (cachedFormula == null) return yOffset;

            // Parse base weapon type
            if (!System.Enum.TryParse<WeaponType>(cachedFormula.BaseWeaponId, out var baseWeaponType))
                return yOffset;

            // Build passive requirements
            var passiveRequirements = BuildPassiveRequirements(cachedFormula);

            // Get sprites
            var baseSprite = DataAccessHelper.GetSpriteForWeapon(baseWeaponType);
            var evolvedSprite = DataAccessHelper.GetSpriteForWeapon(evolvedType);

            // Add section header
            yOffset -= Spacing;
            var headerObj = UIHelper.CreateTextElement(
                parent, "EvolvedFromHeader",
                "Evolved from: (click for details)",
                font, 14f,
                new UnityEngine.Color(0.9f, 0.75f, 0.3f, 1f),
                Il2CppTMPro.FontStyles.Bold
            );
            UIHelper.PositionElement(headerObj, Padding, yOffset, maxWidth - Padding * 2, 20f);
            yOffset -= 22f;

            // Render formula row: [Base Weapon] + [Passive1] + [Passive2]
            bool hasMaxReq = passiveRequirements.Exists(p => p.RequiresMaxLevel);
            float rowHeight = IconSize + 4f + (hasMaxReq ? 12f : 0f);
            float xOffset = Padding + 5f;

            // Base weapon icon
            bool ownsWeapon = DataAccessHelper.PlayerOwnsWeapon(baseWeaponType);
            var weaponIcon = CreateFormulaIcon(
                parent, "EvolvedFromBase",
                baseSprite, ownsWeapon,
                DataAccessHelper.IsWeaponBanned(baseWeaponType),
                IconSize, xOffset, yOffset
            );
            UIHelper.AddWeaponHover(weaponIcon, baseWeaponType, useClick: true);
            xOffset += IconSize + 3f;

            // Passive requirements
            for (int p = 0; p < passiveRequirements.Count; p++)
            {
                var passive = passiveRequirements[p];
                xOffset = RenderPassiveIcon(
                    parent, font, passive, p,
                    xOffset, yOffset, "EvolvedFrom", IconSize
                );
            }

            yOffset -= rowHeight;
            return yOffset;
        }

        /// <summary>
        /// Shows evolutions for passive items (items that enable other weapons to evolve).
        /// Uses cache for O(1) lookup.
        /// </summary>
        public static float AddPassiveEvolutionSection(
            UnityEngine.Transform parent,
            Il2CppTMPro.TMP_FontAsset font,
            WeaponType passiveType,
            float yOffset,
            float maxWidth)
        {
            if (GameDataCache.EvolutionCache == null) return yOffset;

            // Get all formulas that use this weapon as a passive
            var cachedFormulas = GameDataCache.EvolutionCache.GetFormulasUsingWeaponAsPassive(passiveType);
            if (cachedFormulas.Count == 0) return yOffset;

            // Add section header
            yOffset -= Spacing;
            var headerObj = UIHelper.CreateTextElement(
                parent, "EvoHeader",
                "Evolutions: (click for details)",
                font, 14f,
                new UnityEngine.Color(0.9f, 0.75f, 0.3f, 1f),
                Il2CppTMPro.FontStyles.Bold
            );
            UIHelper.PositionElement(headerObj, Padding, yOffset, maxWidth - Padding * 2, 20f);
            yOffset -= 22f;

            // Render each formula
            int formulaIndex = 0;
            foreach (var cachedFormula in cachedFormulas)
            {
                // Parse weapon types
                if (!System.Enum.TryParse<WeaponType>(cachedFormula.BaseWeaponId, out var baseWeaponType))
                    continue;
                if (!System.Enum.TryParse<WeaponType>(cachedFormula.EvolvedWeaponId, out var evolvedWeaponType))
                    continue;

                var passiveRequirements = BuildPassiveRequirements(cachedFormula);
                var baseSprite = DataAccessHelper.GetSpriteForWeapon(baseWeaponType);
                var evolvedSprite = DataAccessHelper.GetSpriteForWeapon(evolvedWeaponType);

                bool hasMaxReq = passiveRequirements.Exists(p => p.RequiresMaxLevel);
                float rowHeight = IconSize + 4f + (hasMaxReq ? 12f : 0f);
                float xOffset = Padding + 5f;
                bool ownsWeapon = DataAccessHelper.PlayerOwnsWeapon(baseWeaponType);

                // Base weapon icon
                var weaponIcon = CreateFormulaIcon(
                    parent, $"Weapon{formulaIndex}",
                    baseSprite, ownsWeapon,
                    DataAccessHelper.IsWeaponBanned(baseWeaponType),
                    IconSize, xOffset, yOffset
                );
                UIHelper.AddWeaponHover(weaponIcon, baseWeaponType, useClick: true);
                xOffset += IconSize + 3f;

                // Passive requirements (skip the one we're viewing)
                for (int p = 0; p < passiveRequirements.Count; p++)
                {
                    var passive = passiveRequirements[p];
                    if (passive.WeaponType.HasValue && passive.WeaponType.Value == passiveType)
                        continue; // Skip redundant passive

                    xOffset = RenderPassiveIcon(
                        parent, font, passive, p,
                        xOffset, yOffset, $"Passive{formulaIndex}_", IconSize
                    );
                }

                // Arrow
                var arrowObj = UIHelper.CreateTextElement(
                    parent, $"Arrow{formulaIndex}", "→",
                    font, 14f,
                    new UnityEngine.Color(0.8f, 0.8f, 0.8f, 1f),
                    Il2CppTMPro.FontStyles.Normal
                );
                UIHelper.PositionElement(arrowObj, xOffset, yOffset - 4f, 20f, IconSize);
                xOffset += 20f;

                // Evolved weapon icon
                var evoIcon = CreateFormulaIcon(
                    parent, $"Evo{formulaIndex}",
                    evolvedSprite, false,
                    DataAccessHelper.IsWeaponBanned(evolvedWeaponType),
                    IconSize, xOffset, yOffset
                );
                UIHelper.AddWeaponHover(evoIcon, evolvedWeaponType, useClick: true);

                yOffset -= rowHeight;
                formulaIndex++;
            }

            return yOffset;
        }

        /// <summary>
        /// Adds evolution section for passive items (ItemType).
        /// Shows all weapons that use this item as a requirement.
        /// </summary>
        public static float AddItemEvolutionSection(
            UnityEngine.Transform parent,
            Il2CppTMPro.TMP_FontAsset font,
            ItemType itemType,
            float yOffset,
            float maxWidth)
        {
            if (GameDataCache.EvolutionCache == null) return yOffset;

            // Get all formulas that use this item as a passive
            var cachedFormulas = GameDataCache.EvolutionCache.GetFormulasUsingItemAsPassive(itemType);
            if (cachedFormulas.Count == 0) return yOffset;

            // Add section header
            yOffset -= Spacing;
            var headerObj = UIHelper.CreateTextElement(
                parent, "EvoHeader",
                "Evolutions: (click for details)",
                font, 14f,
                new UnityEngine.Color(0.9f, 0.75f, 0.3f, 1f),
                Il2CppTMPro.FontStyles.Bold
            );
            UIHelper.PositionElement(headerObj, Padding, yOffset, maxWidth - Padding * 2, 20f);
            yOffset -= 22f;

            // Render each formula
            int formulaIndex = 0;
            foreach (var cachedFormula in cachedFormulas)
            {
                if (!System.Enum.TryParse<WeaponType>(cachedFormula.BaseWeaponId, out var baseWeaponType))
                    continue;
                if (!System.Enum.TryParse<WeaponType>(cachedFormula.EvolvedWeaponId, out var evolvedWeaponType))
                    continue;

                var passiveRequirements = BuildPassiveRequirements(cachedFormula);
                var baseSprite = DataAccessHelper.GetSpriteForWeapon(baseWeaponType);
                var evolvedSprite = DataAccessHelper.GetSpriteForWeapon(evolvedWeaponType);

                bool hasMaxReq = passiveRequirements.Exists(p => p.RequiresMaxLevel);
                float rowHeight = IconSize + 4f + (hasMaxReq ? 12f : 0f);
                float xOffset = Padding + 5f;
                bool ownsWeapon = DataAccessHelper.PlayerOwnsWeapon(baseWeaponType);

                // Base weapon icon
                var weaponIcon = CreateFormulaIcon(
                    parent, $"Weapon{formulaIndex}",
                    baseSprite, ownsWeapon,
                    DataAccessHelper.IsWeaponBanned(baseWeaponType),
                    IconSize, xOffset, yOffset
                );
                UIHelper.AddWeaponHover(weaponIcon, baseWeaponType, useClick: true);
                xOffset += IconSize + 3f;

                // All passive requirements
                for (int p = 0; p < passiveRequirements.Count; p++)
                {
                    var passive = passiveRequirements[p];
                    xOffset = RenderPassiveIcon(
                        parent, font, passive, p,
                        xOffset, yOffset, $"Passive{formulaIndex}_", IconSize
                    );
                }

                // Arrow
                var arrowObj = UIHelper.CreateTextElement(
                    parent, $"Arrow{formulaIndex}", "→",
                    font, 14f,
                    new UnityEngine.Color(0.8f, 0.8f, 0.8f, 1f),
                    Il2CppTMPro.FontStyles.Normal
                );
                UIHelper.PositionElement(arrowObj, xOffset, yOffset - 4f, 20f, IconSize);
                xOffset += 20f;

                // Evolved weapon icon
                var evoIcon = CreateFormulaIcon(
                    parent, $"Evo{formulaIndex}",
                    evolvedSprite, false,
                    DataAccessHelper.IsWeaponBanned(evolvedWeaponType),
                    IconSize, xOffset, yOffset
                );
                UIHelper.AddWeaponHover(evoIcon, evolvedWeaponType, useClick: true);
                xOffset += IconSize + 6f;

                // Evolution name
                var nameObj = UIHelper.CreateTextElement(
                    parent, $"EvoName{formulaIndex}",
                    cachedFormula.EvolvedWeaponName,
                    font, 11f,
                    new UnityEngine.Color(0.75f, 0.75f, 0.8f, 1f),
                    Il2CppTMPro.FontStyles.Normal
                );
                UIHelper.PositionElement(nameObj, xOffset, yOffset - 5f, maxWidth - xOffset - Padding, 16f);

                yOffset -= rowHeight;
                formulaIndex++;
            }

            return yOffset;
        }

        #endregion

        #region Arcana Section

        /// <summary>
        /// Adds a section showing active arcanas that affect a weapon/item.
        /// </summary>
        public static float AddArcanaSection(
            UnityEngine.Transform parent,
            Il2CppTMPro.TMP_FontAsset font,
            List<(string Name, UnityEngine.Sprite Sprite, object ArcanaData)> arcanas,
            float yOffset,
            float maxWidth)
        {
            if (arcanas == null || arcanas.Count == 0) return yOffset;

            // Section header
            yOffset -= Spacing;
            var headerObj = UIHelper.CreateTextElement(
                parent, "ArcanaHeader",
                "Arcana: (click for details)",
                font, 14f,
                new UnityEngine.Color(0.7f, 0.5f, 0.9f, 1f),
                Il2CppTMPro.FontStyles.Bold
            );
            UIHelper.PositionElement(headerObj, Padding, yOffset, maxWidth - Padding * 2, 20f);
            yOffset -= 26f;

            // Display arcana icons
            float iconSize = 52f;
            float xOffset = Padding;

            for (int i = 0; i < arcanas.Count; i++)
            {
                var arcana = arcanas[i];
                var arcanaIcon = CreateFormulaIcon(
                    parent, $"ArcanaIcon{i}",
                    arcana.Sprite, false, false,
                    iconSize, xOffset, yOffset
                );
                UIHelper.AddArcanaHover(arcanaIcon, arcana.ArcanaData);

                // Arcana name next to icon
                var nameObj = UIHelper.CreateTextElement(
                    parent, $"ArcanaName{i}",
                    arcana.Name,
                    font, 13f,
                    new UnityEngine.Color(0.8f, 0.7f, 0.95f, 1f),
                    Il2CppTMPro.FontStyles.Normal
                );
                UIHelper.PositionElement(
                    nameObj,
                    xOffset + iconSize + 8f,
                    yOffset - (iconSize / 2f - 8f),
                    maxWidth - xOffset - iconSize - Padding - 8f,
                    20f
                );

                yOffset -= iconSize + 8f;
            }

            return yOffset;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts cached formula passive requirements to UI-specific PassiveRequirement structs
        /// with sprites and ownership data.
        /// </summary>
        private static List<PassiveRequirement> BuildPassiveRequirements(
            VSItemTooltips.Core.Models.EvolutionFormula cachedFormula)
        {
            var passiveRequirements = new List<PassiveRequirement>();
            if (cachedFormula.RequiredPassives == null) return passiveRequirements;

            foreach (var cachedPassive in cachedFormula.RequiredPassives)
            {
                var uiPassive = new PassiveRequirement
                {
                    RequiresMaxLevel = cachedPassive.RequiresMaxLevel
                };

                if (!string.IsNullOrEmpty(cachedPassive.WeaponId))
                {
                    if (System.Enum.TryParse<WeaponType>(cachedPassive.WeaponId, out var passiveWeaponType))
                    {
                        uiPassive.WeaponType = passiveWeaponType;
                        uiPassive.Sprite = DataAccessHelper.GetSpriteForWeapon(passiveWeaponType);
                        uiPassive.Owned = DataAccessHelper.PlayerOwnsWeapon(passiveWeaponType) ||
                                         DataAccessHelper.PlayerOwnsAccessory(passiveWeaponType);
                    }
                }
                else if (!string.IsNullOrEmpty(cachedPassive.ItemId))
                {
                    if (System.Enum.TryParse<ItemType>(cachedPassive.ItemId, out var passiveItemType))
                    {
                        uiPassive.ItemType = passiveItemType;
                        uiPassive.Sprite = DataAccessHelper.GetSpriteForItem(passiveItemType);
                        uiPassive.Owned = DataAccessHelper.PlayerOwnsItem(passiveItemType);
                    }
                }

                passiveRequirements.Add(uiPassive);
            }

            return passiveRequirements;
        }

        /// <summary>
        /// Renders a complete evolution formula: base weapon + passives → evolved weapon.
        /// </summary>
        private static float RenderEvolutionFormula(
            UnityEngine.Transform parent,
            Il2CppTMPro.TMP_FontAsset font,
            WeaponType baseWeapon,
            List<PassiveRequirement> passives,
            WeaponType evolvedWeapon,
            UnityEngine.Sprite baseSprite,
            UnityEngine.Sprite evolvedSprite,
            float yOffset,
            float maxWidth,
            string headerText)
        {
            // Section header
            yOffset -= Spacing;
            var headerObj = UIHelper.CreateTextElement(
                parent, "EvoHeader", headerText,
                font, 14f,
                new UnityEngine.Color(0.9f, 0.75f, 0.3f, 1f),
                Il2CppTMPro.FontStyles.Bold
            );
            UIHelper.PositionElement(headerObj, Padding, yOffset, maxWidth - Padding * 2, 20f);
            yOffset -= 22f;

            // Calculate row height
            bool hasMaxReq = passives.Exists(p => p.RequiresMaxLevel);
            float rowHeight = IconSize + 8f + (hasMaxReq ? 12f : 0f);
            float xOffset = Padding + 5f;

            // Render passives with plus signs
            for (int i = 0; i < passives.Count; i++)
            {
                var passive = passives[i];

                // Plus sign
                var plusObj = UIHelper.CreateTextElement(
                    parent, $"Plus{i}", "+",
                    font, 18f,
                    new UnityEngine.Color(0.8f, 0.8f, 0.8f, 1f),
                    Il2CppTMPro.FontStyles.Bold
                );
                UIHelper.PositionElement(plusObj, xOffset, yOffset - 8f, 20f, IconSize);
                xOffset += 22f;

                // Passive icon
                bool passiveBanned = passive.WeaponType.HasValue
                    ? DataAccessHelper.IsWeaponBanned(passive.WeaponType.Value)
                    : passive.ItemType.HasValue
                        ? DataAccessHelper.IsItemBanned(passive.ItemType.Value)
                        : false;

                var passiveIcon = CreateFormulaIcon(
                    parent, $"PassiveIcon{i}",
                    passive.Sprite, passive.Owned, passiveBanned,
                    IconSize, xOffset, yOffset
                );

                if (passive.WeaponType.HasValue)
                    UIHelper.AddWeaponHover(passiveIcon, passive.WeaponType.Value, useClick: true);
                else if (passive.ItemType.HasValue)
                    UIHelper.AddItemHover(passiveIcon, passive.ItemType.Value, useClick: true);

                // "MAX" label if required
                if (passive.RequiresMaxLevel)
                {
                    var maxObj = UIHelper.CreateTextElement(
                        parent, $"Max{i}", "MAX",
                        font, 9f,
                        new UnityEngine.Color(1f, 0.85f, 0f, 1f),
                        Il2CppTMPro.FontStyles.Bold
                    );
                    var maxRect = maxObj.GetComponent<UnityEngine.RectTransform>();
                    maxRect.anchorMin = new UnityEngine.Vector2(0f, 1f);
                    maxRect.anchorMax = new UnityEngine.Vector2(0f, 1f);
                    maxRect.pivot = new UnityEngine.Vector2(0.5f, 1f);
                    maxRect.anchoredPosition = new UnityEngine.Vector2(xOffset + IconSize / 2f, yOffset - IconSize);
                    maxRect.sizeDelta = new UnityEngine.Vector2(IconSize, 12f);
                    var maxTmp = maxObj.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
                    if (maxTmp != null) maxTmp.alignment = Il2CppTMPro.TextAlignmentOptions.Center;
                }

                xOffset += IconSize + 4f;
            }

            // Arrow
            var arrowObj = UIHelper.CreateTextElement(
                parent, "Arrow", "→",
                font, 18f,
                new UnityEngine.Color(0.8f, 0.8f, 0.8f, 1f),
                Il2CppTMPro.FontStyles.Normal
            );
            UIHelper.PositionElement(arrowObj, xOffset, yOffset - 8f, 24f, IconSize);
            xOffset += 26f;

            // Evolved weapon icon
            var evoIcon = CreateFormulaIcon(
                parent, "EvoIcon",
                evolvedSprite, false,
                DataAccessHelper.IsWeaponBanned(evolvedWeapon),
                IconSize, xOffset, yOffset
            );
            UIHelper.AddWeaponHover(evoIcon, evolvedWeapon, useClick: true);

            yOffset -= rowHeight;
            return yOffset;
        }

        /// <summary>
        /// Renders a single passive icon with plus sign and optional MAX label.
        /// Returns the new X offset after rendering.
        /// </summary>
        private static float RenderPassiveIcon(
            UnityEngine.Transform parent,
            Il2CppTMPro.TMP_FontAsset font,
            PassiveRequirement passive,
            int index,
            float xOffset,
            float yOffset,
            string namePrefix,
            float iconSize)
        {
            // Plus sign
            var plusObj = UIHelper.CreateTextElement(
                parent, $"{namePrefix}Plus{index}", "+",
                font, 14f,
                new UnityEngine.Color(0.8f, 0.8f, 0.8f, 1f),
                Il2CppTMPro.FontStyles.Bold
            );
            UIHelper.PositionElement(plusObj, xOffset, yOffset - 4f, 14f, iconSize);
            xOffset += 14f;

            // Passive icon
            bool banned = passive.WeaponType.HasValue
                ? DataAccessHelper.IsWeaponBanned(passive.WeaponType.Value)
                : passive.ItemType.HasValue
                    ? DataAccessHelper.IsItemBanned(passive.ItemType.Value)
                    : false;

            var passiveIcon = CreateFormulaIcon(
                parent, $"{namePrefix}Passive{index}",
                passive.Sprite, passive.Owned, banned,
                iconSize, xOffset, yOffset
            );

            if (passive.WeaponType.HasValue)
                UIHelper.AddWeaponHover(passiveIcon, passive.WeaponType.Value, useClick: true);
            else if (passive.ItemType.HasValue)
                UIHelper.AddItemHover(passiveIcon, passive.ItemType.Value, useClick: true);

            // "MAX" label if required
            if (passive.RequiresMaxLevel)
            {
                var maxObj = UIHelper.CreateTextElement(
                    parent, $"{namePrefix}Max{index}", "MAX",
                    font, 9f,
                    new UnityEngine.Color(1f, 0.85f, 0f, 1f),
                    Il2CppTMPro.FontStyles.Bold
                );
                var maxRect = maxObj.GetComponent<UnityEngine.RectTransform>();
                maxRect.anchorMin = new UnityEngine.Vector2(0f, 1f);
                maxRect.anchorMax = new UnityEngine.Vector2(0f, 1f);
                maxRect.pivot = new UnityEngine.Vector2(0.5f, 1f);
                maxRect.anchoredPosition = new UnityEngine.Vector2(xOffset + iconSize / 2f, yOffset - iconSize);
                maxRect.sizeDelta = new UnityEngine.Vector2(iconSize, 12f);
                var maxTmp = maxObj.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
                if (maxTmp != null) maxTmp.alignment = Il2CppTMPro.TextAlignmentOptions.Center;
            }

            xOffset += iconSize + 3f;
            return xOffset;
        }

        /// <summary>
        /// Creates a formula icon with optional ownership highlight and ban overlay.
        /// Delegates to ItemTooltips.CreateFormulaIcon if it's public/accessible,
        /// or provides its own implementation.
        /// </summary>
        private static UnityEngine.GameObject CreateFormulaIcon(
            UnityEngine.Transform parent,
            string name,
            UnityEngine.Sprite sprite,
            bool isOwned,
            bool isBanned,
            float size,
            float x,
            float y)
        {
            var container = new UnityEngine.GameObject(name);
            container.transform.SetParent(parent, false);
            var containerRect = container.AddComponent<UnityEngine.RectTransform>();
            containerRect.anchorMin = new UnityEngine.Vector2(0f, 1f);
            containerRect.anchorMax = new UnityEngine.Vector2(0f, 1f);
            containerRect.pivot = new UnityEngine.Vector2(0f, 1f);
            containerRect.anchoredPosition = new UnityEngine.Vector2(x, y);
            containerRect.sizeDelta = new UnityEngine.Vector2(size, size);

            // Transparent image for raycast
            var containerImage = container.AddComponent<UnityEngine.UI.Image>();
            containerImage.color = new UnityEngine.Color(0f, 0f, 0f, 0f);
            containerImage.raycastTarget = true;

            // Yellow circle for owned items
            if (isOwned)
            {
                var bgObj = new UnityEngine.GameObject("OwnedBg");
                bgObj.transform.SetParent(container.transform, false);
                var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
                bgImage.sprite = DataAccessHelper.GetCircleSprite();
                bgImage.color = new UnityEngine.Color(1f, 0.85f, 0f, 0.7f);
                bgImage.raycastTarget = false;
                var bgRect = bgObj.GetComponent<UnityEngine.RectTransform>();
                bgRect.anchorMin = UnityEngine.Vector2.zero;
                bgRect.anchorMax = UnityEngine.Vector2.one;
                bgRect.offsetMin = new UnityEngine.Vector2(-4f, -4f);
                bgRect.offsetMax = new UnityEngine.Vector2(4f, 4f);
            }

            // Icon sprite
            if (sprite != null)
            {
                var iconObj = new UnityEngine.GameObject("Icon");
                iconObj.transform.SetParent(container.transform, false);
                var iconImage = iconObj.AddComponent<UnityEngine.UI.Image>();
                iconImage.sprite = sprite;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                var iconRect = iconObj.GetComponent<UnityEngine.RectTransform>();
                iconRect.anchorMin = UnityEngine.Vector2.zero;
                iconRect.anchorMax = UnityEngine.Vector2.one;
                iconRect.offsetMin = UnityEngine.Vector2.zero;
                iconRect.offsetMax = UnityEngine.Vector2.zero;
            }

            // Red X for banned items
            if (isBanned)
            {
                for (int barIdx = 0; barIdx < 2; barIdx++)
                {
                    var barObj = new UnityEngine.GameObject(barIdx == 0 ? "BannedBar1" : "BannedBar2");
                    barObj.transform.SetParent(container.transform, false);
                    var barImage = barObj.AddComponent<UnityEngine.UI.Image>();
                    barImage.color = new UnityEngine.Color(1f, 0.15f, 0.15f, 0.9f);
                    barImage.raycastTarget = false;
                    var barRect = barObj.GetComponent<UnityEngine.RectTransform>();
                    barRect.anchorMin = new UnityEngine.Vector2(0.5f, 0.5f);
                    barRect.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);
                    barRect.pivot = new UnityEngine.Vector2(0.5f, 0.5f);
                    barRect.sizeDelta = new UnityEngine.Vector2(size * 1.2f, size * 0.15f);
                    barRect.localRotation = UnityEngine.Quaternion.Euler(0f, 0f, barIdx == 0 ? 45f : -45f);
                }
            }

            return container;
        }

        #endregion
    }

    /// <summary>
    /// Helper methods for UI element creation and positioning.
    /// Keeps Unity-specific code centralized.
    /// </summary>
    internal static class UIHelper
    {
        public static UnityEngine.GameObject CreateTextElement(
            UnityEngine.Transform parent,
            string name,
            string text,
            Il2CppTMPro.TMP_FontAsset font,
            float fontSize,
            UnityEngine.Color color,
            Il2CppTMPro.FontStyles style)
        {
            var obj = new UnityEngine.GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<UnityEngine.RectTransform>();

            var tmp = obj.AddComponent<Il2CppTMPro.TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = Il2CppTMPro.TextAlignmentOptions.Left;

            return obj;
        }

        public static void PositionElement(
            UnityEngine.GameObject obj,
            float x,
            float y,
            float width,
            float height)
        {
            var rect = obj.GetComponent<UnityEngine.RectTransform>();
            if (rect == null) return;

            rect.anchorMin = new UnityEngine.Vector2(0f, 1f);
            rect.anchorMax = new UnityEngine.Vector2(0f, 1f);
            rect.pivot = new UnityEngine.Vector2(0f, 1f);
            rect.anchoredPosition = new UnityEngine.Vector2(x, y);
            rect.sizeDelta = new UnityEngine.Vector2(width, height);
        }

        public static void AddWeaponHover(UnityEngine.GameObject go, WeaponType weaponType, bool useClick)
        {
            ItemTooltipsMod.AddHoverToGameObject(go, weaponType, null, useClick);
        }

        public static void AddItemHover(UnityEngine.GameObject go, ItemType itemType, bool useClick)
        {
            ItemTooltipsMod.AddHoverToGameObject(go, null, itemType, useClick);
        }

        public static void AddArcanaHover(UnityEngine.GameObject go, object arcanaData)
        {
            ItemTooltipsMod.AddArcanaHoverToGameObject(go, arcanaData);
        }
    }
}
