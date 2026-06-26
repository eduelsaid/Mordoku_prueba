using UnityEngine;

namespace Murdoku
{
    [CreateAssetMenu(menuName = "Murdoku/Visuals")]
    public class MurdokuVisuals : ScriptableObject
    {
        [System.Serializable]
        public struct FurnitureSpriteEntry
        {
            public FurnitureType Furniture;
            public Sprite Sprite;
        }

        public FurnitureSpriteEntry[] FurnitureSprites;

        public Sprite GetFurnitureSprite(FurnitureType furniture)
        {
            if (FurnitureSprites == null)
                return null;
            foreach (var entry in FurnitureSprites)
                if (entry.Furniture == furniture)
                    return entry.Sprite;
            return null;
        }
    }
}
