using UnityEngine;
using TMPro;

namespace Components.Game.Items.Scripts
{
    public class ItemAssigner : MonoBehaviour
    {
        [SerializeField] private ItemDatabase itemDatabase;
        
        [Tooltip("初期化時にアサインするアイテムID")]
        [SerializeField] private string initialItemId;

        [Header("UI References (Assign in Inspector)")]
        [Tooltip("ItemHolder (SpriteRenderer component)")]
        [SerializeField] private SpriteRenderer itemHolder;
        [Tooltip("ItemText (TMP_Text component)")]
        [SerializeField] private TMP_Text itemText;

        // 外部公開プロパティ
        public ItemDatabase Database => itemDatabase;
        public string CurrentItemId { get; private set; }

        private void Start()
        {
            if (!string.IsNullOrEmpty(initialItemId))
            {
                AssignItem(initialItemId);
            }
        }

        /// <summary>
        /// IDを指定してアイテムを適用する
        /// </summary>
        /// <param name="itemId">アイテムのID</param>
        public void AssignItem(string itemId)
        {
            CurrentItemId = itemId;

            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase is not assigned in ItemAssigner.");
                return;
            }

            var itemData = itemDatabase.GetItem(itemId);
            if (itemData != null)
            {
                if (itemHolder != null)
                {
                    itemHolder.sprite = itemData.icon;
                    // 画像がない場合は透明にするなどの処理が必要なら記述
                    itemHolder.enabled = (itemData.icon != null);
                }

                if (itemText != null)
                {
                    // "+<数値>" の形式で表示
                    itemText.text = "+" + itemData.timeReduction.ToString();
                }
            }
            else
            {
                Debug.LogWarning($"Item with ID '{itemId}' not found in database.");
            }
        }
    }
}
