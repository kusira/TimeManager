using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Components.Game.Items.Scripts
{
    public class ItemUser : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("ドラッグ判定に使用するコライダーを持つゲームオブジェクト")]
        [SerializeField] private Collider2D dragCollider;

        private ItemAssigner itemAssigner;
        private Vector3 startPosition;
        private bool isUsing = false; // 使用中フラグ

        private void Awake()
        {
            itemAssigner = GetComponent<ItemAssigner>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isUsing) return;

            startPosition = transform.position;
            
            if (dragCollider != null)
            {
                dragCollider.enabled = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isUsing) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
            mousePos.z = 0; 
            transform.position = mousePos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isUsing) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                string tag = hit.collider.tag;

                if (tag.StartsWith("Worker"))
                {
                    int workerIndex = GetWorkerIndexFromTag(tag);
                    if (workerIndex != -1)
                    {
                        // アイテム使用処理開始（コルーチン）
                        StartCoroutine(UseItemAnimation(workerIndex));
                        return;
                    }
                }
            }
            
            if (dragCollider != null)
            {
                dragCollider.enabled = true;
            }
            transform.position = startPosition;
        }

        private IEnumerator UseItemAnimation(int workerIndex)
        {
            isUsing = true;
            
            // 効果適用
            ApplyItemToWorker(workerIndex);

            // 0.1秒のアニメーション（中間フレームを作成）
            // 例: スケールを少し大きくしてから消す、など
            float duration = 0.1f;
            float halfDuration = duration / 2f;
            Vector3 initialScale = transform.localScale;
            Vector3 midScale = initialScale * 1.2f; // 中間フレームで少し大きくする
            Vector3 endScale = Vector3.zero;

            float timer = 0f;

            // 前半: 初期スケール -> 中間スケール
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / halfDuration);
                transform.localScale = Vector3.Lerp(initialScale, midScale, t);
                yield return null;
            }

            // 後半: 中間スケール -> 0
            timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / halfDuration);
                transform.localScale = Vector3.Lerp(midScale, endScale, t);
                yield return null;
            }

            Destroy(gameObject);
        }

        private int GetWorkerIndexFromTag(string tag)
        {
            if (tag.Length > 0)
            {
                char suffix = tag[tag.Length - 1];
                int index = suffix - 'A';
                if (index >= 0 && index < 26) 
                {
                    return index;
                }
            }
            return -1;
        }

        private void ApplyItemToWorker(int workerIndex)
        {
            if (itemAssigner == null || itemAssigner.Database == null) return;
            
            string currentId = itemAssigner.CurrentItemId;
            if (string.IsNullOrEmpty(currentId)) return;

            var itemData = itemAssigner.Database.GetItem(currentId);
            if (itemData == null) return;

            var progresser = FindFirstObjectByType<Graph.Scripts.TaskProgresser>();
            if (progresser != null)
            {
                progresser.ReduceTaskTime(workerIndex, itemData.timeReduction);
            }
        }
    }
}
