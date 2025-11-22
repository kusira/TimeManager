using System.Collections.Generic;
using UnityEngine;

namespace Components.Game.Items.Scripts
{
    [CreateAssetMenu(fileName = "NewItemDatabase", menuName = "Game/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [System.Serializable]
        public class ItemData
        {
            public string id;
            public Sprite icon;
            [Tooltip("仕事短縮時間")]
            public float timeReduction;
        }

        [SerializeField]
        private List<ItemData> items = new List<ItemData>();

        public ItemData GetItem(string id)
        {
            return items.Find(item => item.id == id);
        }
    }
}
