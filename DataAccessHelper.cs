using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;
using MelonLoader;

namespace VSItemTooltips
{
    /// <summary>
    /// Provides centralized data access for weapons, items, sprites, localization, and ownership.
    /// All methods are static for easy access throughout the mod.
    /// </summary>
    public static class DataAccessHelper
    {
        #region Cached Resources

        private static System.Type spriteManagerType = null;
        private static bool spriteManagerDebugLogged = false;
        private static UnityEngine.Sprite cachedCircleSprite = null;
        private static bool spriteLoadDebugLogged = false;

        #endregion

        #region Weapon/Item Data Access

        /// <summary>
        /// Gets the first WeaponData for a given weapon type from the weapons dictionary.
        /// </summary>
        public static WeaponData GetWeaponData(WeaponType type)
        {
            if (GameDataCache.WeaponsDict == null) return null;

            try
            {
                var dictType = GameDataCache.WeaponsDict.GetType();
                var containsMethod = dictType.GetMethod("ContainsKey");
                if (containsMethod != null && (bool)containsMethod.Invoke(GameDataCache.WeaponsDict, new object[] { type }))
                {
                    var indexer = dictType.GetProperty("Item");
                    if (indexer != null)
                    {
                        // Dictionary value is List<WeaponData>, get the first item
                        var list = indexer.GetValue(GameDataCache.WeaponsDict, new object[] { type }) as Il2CppSystem.Collections.Generic.List<WeaponData>;
                        if (list != null && list.Count > 0)
                        {
                            return list[0];
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Gets the full list of WeaponData for a given weapon type.
        /// </summary>
        public static Il2CppSystem.Collections.Generic.List<WeaponData> GetWeaponDataList(WeaponType type)
        {
            if (GameDataCache.WeaponsDict == null) return null;
            try
            {
                var indexer = GameDataCache.WeaponsDict.GetType().GetProperty("Item");
                if (indexer != null)
                {
                    return indexer.GetValue(GameDataCache.WeaponsDict, new object[] { type }) as Il2CppSystem.Collections.Generic.List<WeaponData>;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Gets the PowerUpData for a given item type from the powerups dictionary.
        /// </summary>
        public static object GetPowerUpData(ItemType type)
        {
            if (GameDataCache.PowerUpsDict == null) return null;

            try
            {
                var dictType = GameDataCache.PowerUpsDict.GetType();
                var containsMethod = dictType.GetMethod("ContainsKey");
                if (containsMethod != null && (bool)containsMethod.Invoke(GameDataCache.PowerUpsDict, new object[] { type }))
                {
                    var indexer = dictType.GetProperty("Item");
                    if (indexer != null)
                    {
                        var listObj = indexer.GetValue(GameDataCache.PowerUpsDict, new object[] { type });
                        // Dictionary value is List<PowerUpData>, get the first item
                        if (listObj != null)
                        {
                            var countProp = listObj.GetType().GetProperty("Count");
                            if (countProp != null && (int)countProp.GetValue(listObj) > 0)
                            {
                                var itemIndexer = listObj.GetType().GetProperty("Item");
                                if (itemIndexer != null)
                                    return itemIndexer.GetValue(listObj, new object[] { 0 });
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        #endregion

        #region Sprite Loading

        /// <summary>
        /// Creates or returns a cached circular sprite for highlighting owned items.
        /// </summary>
        public static UnityEngine.Sprite GetCircleSprite()
        {
            if (cachedCircleSprite != null)
                return cachedCircleSprite;

            try
            {
                // Create a circular texture
                int size = 64;
                var texture = new UnityEngine.Texture2D(size, size, UnityEngine.TextureFormat.RGBA32, false);
                texture.filterMode = UnityEngine.FilterMode.Bilinear;

                float center = size / 2f;
                float radius = center - 1f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - center;
                        float dy = y - center;
                        float dist = UnityEngine.Mathf.Sqrt(dx * dx + dy * dy);

                        if (dist <= radius)
                        {
                            // Inside circle - white with soft edge
                            float alpha = 1f;
                            if (dist > radius - 2f)
                            {
                                // Soft edge for anti-aliasing
                                alpha = (radius - dist) / 2f;
                            }
                            texture.SetPixel(x, y, new UnityEngine.Color(1f, 1f, 1f, alpha));
                        }
                        else
                        {
                            // Outside circle - transparent
                            texture.SetPixel(x, y, new UnityEngine.Color(0f, 0f, 0f, 0f));
                        }
                    }
                }

                texture.Apply();

                // Create sprite from texture
                cachedCircleSprite = UnityEngine.Sprite.Create(
                    texture,
                    new UnityEngine.Rect(0, 0, size, size),
                    new UnityEngine.Vector2(0.5f, 0.5f),
                    100f
                );

                return cachedCircleSprite;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Error creating circle sprite: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads a sprite from an atlas using SpriteManager.GetSpriteFast.
        /// </summary>
        public static UnityEngine.Sprite LoadSpriteFromAtlas(string frameName, string atlasName)
        {
            try
            {
                // Initialize spriteManagerType if needed
                if (spriteManagerType == null)
                {
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        spriteManagerType = assembly.GetTypes().FirstOrDefault(t => t.Name == "SpriteManager");
                        if (spriteManagerType != null)
                        {
                            if (!spriteManagerDebugLogged)
                            {
                                spriteManagerDebugLogged = true;
                            }
                            break;
                        }
                    }
                    if (spriteManagerType == null && !spriteManagerDebugLogged)
                    {
                        MelonLogger.Warning("[LoadSpriteFromAtlas] SpriteManager type not found!");
                        spriteManagerDebugLogged = true;
                    }
                }

                if (spriteManagerType == null) return null;

                var getSpriteFastMethod = spriteManagerType.GetMethod("GetSpriteFast",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new System.Type[] { typeof(string), typeof(string) },
                    null);

                if (getSpriteFastMethod != null)
                {
                    var result = getSpriteFastMethod.Invoke(null, new object[] { frameName, atlasName }) as UnityEngine.Sprite;
                    if (result != null) return result;

                    // Try without extension
                    if (frameName.Contains("."))
                    {
                        var nameWithoutExt = frameName.Substring(0, frameName.LastIndexOf('.'));
                        result = getSpriteFastMethod.Invoke(null, new object[] { nameWithoutExt, atlasName }) as UnityEngine.Sprite;
                    }
                    return result;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Gets the sprite for a weapon type by loading from the weapon's texture atlas.
        /// </summary>
        public static UnityEngine.Sprite GetSpriteForWeapon(WeaponType weaponType)
        {
            var data = GetWeaponData(weaponType);
            if (data == null)
            {
                if (!spriteLoadDebugLogged)
                {
                }
                return null;
            }

            try
            {
                string frameName = data.frameName;
                // Property is "texture" not "textureName"
                string atlasName = GetPropertyValue<string>(data, "texture");

                if (!spriteLoadDebugLogged)
                {
                    spriteLoadDebugLogged = true;
                }

                if (!string.IsNullOrEmpty(frameName) && !string.IsNullOrEmpty(atlasName))
                {
                    return LoadSpriteFromAtlas(frameName, atlasName);
                }

                // Fallback: try common atlas names
                if (!string.IsNullOrEmpty(frameName))
                {
                    string[] fallbackAtlases = { "weapons", "items", "characters", "ui" };
                    foreach (var atlas in fallbackAtlases)
                    {
                        var sprite = LoadSpriteFromAtlas(frameName, atlas);
                        if (sprite != null) return sprite;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[GetSpriteForWeapon] Error: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Gets the sprite for an item type by loading from the item's texture atlas.
        /// </summary>
        public static UnityEngine.Sprite GetSpriteForItem(ItemType itemType)
        {
            var data = GetPowerUpData(itemType);
            if (data == null) return null;

            try
            {
                string frameName = GetPropertyValue<string>(data, "frameName");
                // Property is "texture" not "textureName"
                string atlasName = GetPropertyValue<string>(data, "texture");

                if (!string.IsNullOrEmpty(frameName) && !string.IsNullOrEmpty(atlasName))
                {
                    return LoadSpriteFromAtlas(frameName, atlasName);
                }

                // Fallback: try common atlas names
                if (!string.IsNullOrEmpty(frameName))
                {
                    string[] fallbackAtlases = { "items", "powerups", "weapons", "ui" };
                    foreach (var atlas in fallbackAtlases)
                    {
                        var sprite = LoadSpriteFromAtlas(frameName, atlas);
                        if (sprite != null) return sprite;
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Loads an arcana sprite from texture and frame name.
        /// </summary>
        public static UnityEngine.Sprite LoadArcanaSprite(string textureName, string frameName)
        {
            string cleanFrameName = frameName;
            if (cleanFrameName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                cleanFrameName = cleanFrameName.Substring(0, cleanFrameName.Length - 4);
            }

            // Try the specific texture atlas first
            if (!string.IsNullOrEmpty(textureName))
            {
                string[] frameNamesToTry = { frameName, cleanFrameName, $"{cleanFrameName}.png" };
                foreach (var fn in frameNamesToTry)
                {
                    var sprite = LoadSpriteFromAtlas(fn, textureName);
                    if (sprite != null) return sprite;
                }
            }

            // Search all loaded sprites
            var allSprites = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Sprite>();

            foreach (var s in allSprites)
            {
                if (s == null || s.texture == null) continue;

                string texName = s.texture.name.ToLower();
                string spriteName = s.name.ToLower();

                if (!string.IsNullOrEmpty(textureName) && texName.Contains(textureName.ToLower()) &&
                    (spriteName == cleanFrameName.ToLower() || spriteName == frameName.ToLower()))
                {
                    return s;
                }
            }

            // Fallback atlases
            string[] fallbackAtlases = { "arcanas", "cards", "items", "ui", "randomazzo" };
            foreach (var atlas in fallbackAtlases)
            {
                var sprite = LoadSpriteFromAtlas(cleanFrameName, atlas);
                if (sprite != null) return sprite;
                sprite = LoadSpriteFromAtlas(frameName, atlas);
                if (sprite != null) return sprite;
            }

            return null;
        }

        #endregion

        #region Localization

        /// <summary>
        /// Gets the localized description for a weapon.
        /// </summary>
        public static string GetLocalizedWeaponDescription(WeaponData data, WeaponType type)
        {
            if (data == null) return "";

            // Try GetLocalizedDescriptionTerm + I2 translation first (gives flavor text description)
            try
            {
                var termMethod = data.GetType().GetMethod("GetLocalizedDescriptionTerm", BindingFlags.Public | BindingFlags.Instance);
                if (termMethod != null)
                {
                    var term = termMethod.Invoke(data, new object[] { type }) as string;
                    if (!string.IsNullOrEmpty(term))
                    {
                        var translated = GetI2Translation(term);
                        if (!string.IsNullOrEmpty(translated)) return translated;
                    }
                }
            }
            catch { }

            // Fallback to raw description field
            if (!string.IsNullOrEmpty(data.description))
                return data.description;

            return "";
        }

        /// <summary>
        /// Gets the localized name for a weapon.
        /// </summary>
        public static string GetLocalizedWeaponName(WeaponData data, WeaponType type)
        {
            if (data == null) return type.ToString();
            try
            {
                var method = data.GetType().GetMethod("GetLocalizedNameTerm", BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    var term = method.Invoke(data, new object[] { type }) as string;
                    if (!string.IsNullOrEmpty(term))
                    {
                        var translated = GetI2Translation(term);
                        if (!string.IsNullOrEmpty(translated)) return translated;
                    }
                }
            }
            catch { }
            return data.name ?? type.ToString();
        }

        /// <summary>
        /// Gets the localized description for a power-up/item.
        /// </summary>
        public static string GetLocalizedPowerUpDescription(object data, ItemType type)
        {
            if (data == null) return "";
            try
            {
                // GetLocalizedDescription is an instance method that takes PowerUpType (not ItemType)
                // PowerUpType and ItemType may share the same underlying enum values
                var method = data.GetType().GetMethod("GetLocalizedDescription", BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    // Need to convert ItemType to the parameter type the method expects
                    var paramType = method.GetParameters()[0].ParameterType;
                    object convertedType = Enum.ToObject(paramType, (int)type);
                    var result = method.Invoke(data, new object[] { convertedType }) as string;
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            }
            catch { }
            return GetPropertyValue<string>(data, "description") ?? "";
        }

        /// <summary>
        /// Gets the localized name for a power-up/item.
        /// </summary>
        public static string GetLocalizedPowerUpName(object data, ItemType type)
        {
            if (data == null) return type.ToString();
            try
            {
                var method = data.GetType().GetMethod("GetLocalizedName", BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    var paramType = method.GetParameters()[0].ParameterType;
                    object convertedType = Enum.ToObject(paramType, (int)type);
                    var result = method.Invoke(data, new object[] { convertedType }) as string;
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            }
            catch { }
            return GetPropertyValue<string>(data, "name") ?? type.ToString();
        }

        /// <summary>
        /// Gets a translated string from I2 Localization.
        /// </summary>
        public static string GetI2Translation(string term)
        {
            if (string.IsNullOrEmpty(term)) return null;
            try
            {
                var locType = System.Type.GetType("Il2CppI2.Loc.LocalizationManager, Il2Cppl2localization");
                if (locType != null)
                {
                    var method = locType.GetMethod("GetTranslation", BindingFlags.Public | BindingFlags.Static);
                    if (method != null)
                    {
                        var result = method.Invoke(null, new object[] { term, false, 0, false, true, null, null, false }) as string;
                        return result;
                    }
                }
            }
            catch { }
            return null;
        }

        #endregion

        #region Ownership Checks

        /// <summary>
        /// Checks if the player owns a specific weapon type.
        /// </summary>
        public static bool PlayerOwnsWeapon(WeaponType weaponType)
        {
            if (GameDataCache.GameSession == null)
            {
                return false;
            }

            try
            {
                var activeCharProp = GameDataCache.GameSession.GetType().GetProperty("ActiveCharacter", BindingFlags.Public | BindingFlags.Instance);
                if (activeCharProp == null) return false;

                var activeChar = activeCharProp.GetValue(GameDataCache.GameSession);
                if (activeChar == null) return false;

                var weaponsManagerProp = activeChar.GetType().GetProperty("WeaponsManager", BindingFlags.Public | BindingFlags.Instance);
                if (weaponsManagerProp == null) return false;

                var weaponsManager = weaponsManagerProp.GetValue(activeChar);
                if (weaponsManager == null) return false;

                var activeEquipProp = weaponsManager.GetType().GetProperty("ActiveEquipment", BindingFlags.Public | BindingFlags.Instance);
                if (activeEquipProp == null) return false;

                var equipment = activeEquipProp.GetValue(weaponsManager);
                if (equipment == null) return false;

                // Iterate through equipment list
                var countProp = equipment.GetType().GetProperty("Count");
                var indexer = equipment.GetType().GetProperty("Item");
                if (countProp == null || indexer == null) return false;

                int count = (int)countProp.GetValue(equipment);
                string searchStr = weaponType.ToString();

                for (int i = 0; i < count; i++)
                {
                    var item = indexer.GetValue(equipment, new object[] { i });
                    if (item == null) continue;

                    var typeProp = item.GetType().GetProperty("Type", BindingFlags.Public | BindingFlags.Instance);
                    if (typeProp != null)
                    {
                        var itemType = typeProp.GetValue(item);
                        if (itemType != null)
                        {
                            string itemTypeStr = itemType.ToString();
                            if (itemTypeStr == searchStr)
                            {
                                return true;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[PlayerOwnsWeapon] Error checking {weaponType}: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Checks if the player owns a specific item type (passive item).
        /// </summary>
        public static bool PlayerOwnsItem(ItemType itemType)
        {
            if (GameDataCache.GameSession == null) return false;

            try
            {
                var activeCharProp = GameDataCache.GameSession.GetType().GetProperty("ActiveCharacter", BindingFlags.Public | BindingFlags.Instance);
                if (activeCharProp == null) return false;

                var activeChar = activeCharProp.GetValue(GameDataCache.GameSession);
                if (activeChar == null) return false;

                var accessoriesManagerProp = activeChar.GetType().GetProperty("AccessoriesManager", BindingFlags.Public | BindingFlags.Instance);
                if (accessoriesManagerProp == null) return false;

                var accessoriesManager = accessoriesManagerProp.GetValue(activeChar);
                if (accessoriesManager == null) return false;

                var activeEquipProp = accessoriesManager.GetType().GetProperty("ActiveEquipment", BindingFlags.Public | BindingFlags.Instance);
                if (activeEquipProp == null) return false;

                var equipment = activeEquipProp.GetValue(accessoriesManager);
                if (equipment == null) return false;

                // Iterate through equipment list
                var countProp = equipment.GetType().GetProperty("Count");
                var indexer = equipment.GetType().GetProperty("Item");
                if (countProp == null || indexer == null) return false;

                int count = (int)countProp.GetValue(equipment);
                for (int i = 0; i < count; i++)
                {
                    var item = indexer.GetValue(equipment, new object[] { i });
                    if (item == null) continue;

                    var typeProp = item.GetType().GetProperty("Type", BindingFlags.Public | BindingFlags.Instance);
                    if (typeProp != null)
                    {
                        var equipType = typeProp.GetValue(item);
                        if (equipType != null && equipType.ToString() == itemType.ToString())
                            return true;
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Checks if a WeaponType is equipped as an accessory (passive item).
        /// Passive items like Wings have WeaponType entries but are in the accessories slot.
        /// </summary>
        public static bool PlayerOwnsAccessory(WeaponType weaponType)
        {
            if (GameDataCache.GameSession == null) return false;

            try
            {
                var activeCharProp = GameDataCache.GameSession.GetType().GetProperty("ActiveCharacter", BindingFlags.Public | BindingFlags.Instance);
                if (activeCharProp == null) return false;

                var activeChar = activeCharProp.GetValue(GameDataCache.GameSession);
                if (activeChar == null) return false;

                var accessoriesManagerProp = activeChar.GetType().GetProperty("AccessoriesManager", BindingFlags.Public | BindingFlags.Instance);
                if (accessoriesManagerProp == null) return false;

                var accessoriesManager = accessoriesManagerProp.GetValue(activeChar);
                if (accessoriesManager == null) return false;

                var activeEquipProp = accessoriesManager.GetType().GetProperty("ActiveEquipment", BindingFlags.Public | BindingFlags.Instance);
                if (activeEquipProp == null) return false;

                var equipment = activeEquipProp.GetValue(accessoriesManager);
                if (equipment == null) return false;

                var countProp = equipment.GetType().GetProperty("Count");
                var indexer = equipment.GetType().GetProperty("Item");
                if (countProp == null || indexer == null) return false;

                int count = (int)countProp.GetValue(equipment);
                string searchStr = weaponType.ToString();

                for (int i = 0; i < count; i++)
                {
                    var item = indexer.GetValue(equipment, new object[] { i });
                    if (item == null) continue;

                    var typeProp = item.GetType().GetProperty("Type", BindingFlags.Public | BindingFlags.Instance);
                    if (typeProp != null)
                    {
                        var equipType = typeProp.GetValue(item);
                        if (equipType != null && equipType.ToString() == searchStr)
                            return true;
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Gets all weapon types owned by the player.
        /// </summary>
        public static List<WeaponType> GetOwnedWeaponTypes()
        {
            var owned = new List<WeaponType>();
            try
            {
                foreach (WeaponType wt in Enum.GetValues(typeof(WeaponType)))
                {
                    if (PlayerOwnsWeapon(wt))
                        owned.Add(wt);
                }
            }
            catch { }
            return owned;
        }

        /// <summary>
        /// Gets all item types owned by the player.
        /// </summary>
        public static List<ItemType> GetOwnedItemTypes()
        {
            var owned = new List<ItemType>();
            try
            {
                foreach (ItemType it in Enum.GetValues(typeof(ItemType)))
                {
                    if (PlayerOwnsItem(it))
                        owned.Add(it);
                }
            }
            catch { }
            return owned;
        }

        #endregion

        #region Ban Checks

        /// <summary>
        /// Checks if a weapon type has been banished by the player.
        /// </summary>
        public static bool IsWeaponBanned(WeaponType weaponType)
        {
            try
            {
                var levelUpFactory = GameDataCache.GetLevelUpFactory();
                if (levelUpFactory == null) return false;

                var banishedProp = levelUpFactory.GetType().GetProperty("BanishedWeapons",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (banishedProp == null) return false;

                var banishedWeapons = banishedProp.GetValue(levelUpFactory);
                if (banishedWeapons == null) return false;

                // Check if it contains the weapon type
                var containsMethod = banishedWeapons.GetType().GetMethod("Contains");
                if (containsMethod != null)
                {
                    return (bool)containsMethod.Invoke(banishedWeapons, new object[] { weaponType });
                }

                // Fallback: iterate with Count + Item
                var countProp = banishedWeapons.GetType().GetProperty("Count");
                var indexer = banishedWeapons.GetType().GetProperty("Item");
                if (countProp == null || indexer == null) return false;

                int count = (int)countProp.GetValue(banishedWeapons);
                string searchStr = weaponType.ToString();

                for (int i = 0; i < count; i++)
                {
                    var item = indexer.GetValue(banishedWeapons, new object[] { i });
                    if (item != null && item.ToString() == searchStr)
                        return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Checks if an item type has been banished by the player.
        /// </summary>
        public static bool IsItemBanned(ItemType itemType)
        {
            try
            {
                var levelUpFactory = GameDataCache.GetLevelUpFactory();
                if (levelUpFactory == null) return false;

                // Try BanishedPowerUps first, then BanishedItems
                string[] propNames = { "BanishedPowerUps", "BanishedItems" };
                foreach (var propName in propNames)
                {
                    var banishedProp = levelUpFactory.GetType().GetProperty(propName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (banishedProp == null) continue;

                    var banishedItems = banishedProp.GetValue(levelUpFactory);
                    if (banishedItems == null) continue;

                    var containsMethod = banishedItems.GetType().GetMethod("Contains");
                    if (containsMethod != null)
                    {
                        try { return (bool)containsMethod.Invoke(banishedItems, new object[] { itemType }); }
                        catch { }
                    }

                    // Fallback: iterate
                    var countProp = banishedItems.GetType().GetProperty("Count");
                    var indexer = banishedItems.GetType().GetProperty("Item");
                    if (countProp == null || indexer == null) continue;

                    int count = (int)countProp.GetValue(banishedItems);
                    string searchStr = itemType.ToString();

                    for (int i = 0; i < count; i++)
                    {
                        var item = indexer.GetValue(banishedItems, new object[] { i });
                        if (item != null && item.ToString() == searchStr)
                            return true;
                    }
                }
            }
            catch { }

            return false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets a property or field value from an object via reflection.
        /// </summary>
        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null) return default;

            try
            {
                var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                    return (T)prop.GetValue(obj);

                var field = obj.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                    return (T)field.GetValue(obj);
            }
            catch { }

            return default;
        }

        /// <summary>
        /// Gets a TMP font from an existing TextMeshProUGUI component.
        /// </summary>
        public static Il2CppTMPro.TMP_FontAsset GetFont()
        {
            // Try to find an existing TMP text component to get its font
            var existingTmp = UnityEngine.Object.FindObjectOfType<Il2CppTMPro.TextMeshProUGUI>();
            return existingTmp?.font;
        }

        #endregion
    }
}
