using UnityEngine;
using Il2CppTMPro;

namespace VSItemTooltips.Core.Abstractions
{
    /// <summary>
    /// Abstraction for Unity UI creation operations.
    /// Allows mocking UI operations in tests.
    /// </summary>
    public interface IUnityUIFactory
    {
        GameObject CreateGameObject(string name);
        RectTransform AddRectTransform(GameObject go);
        UnityEngine.UI.Image AddImage(GameObject go);
        TextMeshProUGUI AddTextMeshPro(GameObject go);
        UnityEngine.UI.Button AddButton(GameObject go);
        UnityEngine.EventSystems.EventTrigger AddEventTrigger(GameObject go);
        UnityEngine.UI.Outline AddOutline(GameObject go);
        UnityEngine.UI.ContentSizeFitter AddContentSizeFitter(GameObject go);
        
        void SetParent(Transform child, Transform parent, bool worldPositionStays);
        void DestroyGameObject(GameObject go);
        
        Sprite CreateSprite(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit);
        Texture2D CreateTexture(int width, int height, TextureFormat format, bool mipChain);
    }
}
