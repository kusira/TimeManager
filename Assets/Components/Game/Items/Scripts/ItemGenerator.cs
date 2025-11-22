using System.Collections.Generic;
using UnityEngine;

namespace Components.Game.Items.Scripts
{
    public class ItemGenerator : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("X軸の間隔")]
        [SerializeField] private float spacingX = 2.0f;
        
        [Tooltip("生成するアイテムPrefab")]
        [SerializeField] private GameObject itemPrefab;
        
        [Tooltip("生成するアイテムPrefabの親ゲームオブジェクト")]
        [SerializeField] private Transform itemParent;

        [Header("Data")]
        [Tooltip("生成するアイテムのIDリスト")]
        [SerializeField] private List<string> itemIds = new List<string>();

        void Start()
        {
          GenerateItems();
        }

        /// <summary>
        /// アイテムを生成します
        /// </summary>
        public void GenerateItems()
        {
            if (itemPrefab == null)
            {
                Debug.LogError("Item Prefab is not assigned!");
                return;
            }
            if (itemParent == null)
            {
                Debug.LogError("Item Parent is not assigned!");
                return;
            }

            // 既存のアイテムをクリア (親オブジェクトの子を全削除)
            // エディタスクリプトからの呼び出しを考慮してDestroyImmediateを使用
            var children = new List<GameObject>();
            foreach (Transform child in itemParent)
            {
                children.Add(child.gameObject);
            }
            foreach (var child in children)
            {
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }

            // アイテム生成
            
            // リクエスト: "X軸の間隔"
            // 実装: 中心を基準に並べる (Centered) のが見栄えが良いことが多いので、
            // 全体の幅 (N-1)*Spacing を計算し、-Width/2 から開始します。

            float totalWidth = (itemIds.Count - 1) * spacingX;
            float startX = -totalWidth / 2.0f;

            for (int i = 0; i < itemIds.Count; i++)
            {
                string itemId = itemIds[i];
                
                GameObject itemObj = Instantiate(itemPrefab, itemParent);
                itemObj.name = $"Item_{i}_{itemId}";

                // 座標設定
                float x = startX + (i * spacingX);
                itemObj.transform.localPosition = new Vector3(x, 0, 0);

                // ItemAssignerを使って初期化
                var assigner = itemObj.GetComponent<ItemAssigner>();
                if (assigner != null)
                {
                    assigner.AssignItem(itemId);
                }
                else
                {
                    Debug.LogWarning("ItemPrefab does not have ItemAssigner component.");
                }
            }
        }
    }
}

