using UnityEngine;
using TMPro;

namespace Components.Game.Items.Scripts
{
    public class ItemAssigner : MonoBehaviour
    {
        [SerializeField] private ItemDatabase itemDatabase;
        
        [Tooltip("初期化時にアサインするアイテムID")]
        [SerializeField] private string initialItemId;

        [Tooltip("初期個数")]
        [SerializeField] private int initialCount = 1;

        [Header("UI References (Assign in Inspector)")]
        [Tooltip("ItemHolder (SpriteRenderer component)")]
        [SerializeField] private SpriteRenderer itemHolder;
        
        [Tooltip("ItemText (Game Object with TMP_Text component or TMP_Text directly)")]
        [SerializeField] private GameObject itemTextObject; // Changed from TMP_Text to GameObject
        private TMP_Text itemText;

        [Tooltip("CountText (Game Object with TMP_Text component or TMP_Text directly)")]
        [SerializeField] private GameObject countTextObject; // Changed from TMP_Text to GameObject
        private TMP_Text countText;

        // 外部公開プロパティ
        public ItemDatabase Database => itemDatabase;
        public string CurrentItemId { get; private set; }
        public int CurrentCount { get; private set; }

        private void Awake()
        {
            // Resolve TMP components from GameObjects if assigned
            if (itemTextObject != null)
            {
                itemText = itemTextObject.GetComponent<TMP_Text>();
                if (itemText == null) Debug.LogWarning("ItemTextObject assigned but no TMP_Text component found.");
            }

            if (countTextObject != null)
            {
                countText = countTextObject.GetComponent<TMP_Text>();
                if (countText == null) Debug.LogWarning("CountTextObject assigned but no TMP_Text component found.");
            }
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(initialItemId))
            {
                AssignItem(initialItemId, initialCount);
            }
        }

        /// <summary>
        /// IDと個数を指定してアイテムを適用する
        /// </summary>
        /// <param name="itemId">アイテムのID</param>
        /// <param name="count">アイテムの個数</param>
        public void AssignItem(string itemId, int count)
        {
            CurrentItemId = itemId;
            CurrentCount = count;

            UpdateVisuals();
        }

        /// <summary>
        /// アイテムの個数を減らす
        /// </summary>
        /// <returns>残り個数</returns>
        public int DecrementCount()
        {
            CurrentCount--;
            UpdateVisuals();
            return CurrentCount;
        }

        private void UpdateVisuals()
        {
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase is not assigned in ItemAssigner.");
                return;
            }

            var itemData = itemDatabase.GetItem(CurrentItemId);
            if (itemData != null)
            {
                if (itemHolder != null)
                {
                    itemHolder.sprite = itemData.icon;
                    itemHolder.enabled = (itemData.icon != null);
                }

                if (itemText != null)
                {
                    // "+<数値>" の形式で表示
                    itemText.text = "+" + itemData.timeReduction.ToString();
                }

                if (countText != null)
                {
                    // "x<個数>" の形式で表示
                    countText.text = "x" + CurrentCount.ToString();
                }
            }
            else
            {
                // データがない場合などの処理
                if(CurrentItemId != null) Debug.LogWarning($"Item with ID '{CurrentItemId}' not found in database.");
            }
        }
    }
}
