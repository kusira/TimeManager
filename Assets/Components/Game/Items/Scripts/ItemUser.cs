using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // ItemAssignerのCountTextを操作するために必要かもしれないが、ItemAssigner経由で行う

namespace Components.Game.Items.Scripts
{
    public class ItemUser : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("ドラッグ判定に使用するコライダーを持つゲームオブジェクト")]
        [SerializeField] private Collider2D dragCollider;

        [Header("Hover Settings")]
        [Tooltip("ホバー時のスケール倍率")]
        [SerializeField] private float hoverScale = 1.2f;
        [Tooltip("ホバーアニメーションの時間")]
        [SerializeField] private float hoverAnimDuration = 0.3f;

        private ItemAssigner itemAssigner;
        private Vector3 startPosition;
        private Vector3 initialScale;
        private bool isUsing = false; // 使用中フラグ

        // ドラッグ中のオブジェクト（自分自身か、生成したコピーか）
        private GameObject draggingObject;
        private bool isDraggingCopy = false;

        // ホバー制御用のクラス（ターゲットごとの状態保持）
        private class HoverState
        {
            public Transform target;
            public Vector3 originalScale;
            public Coroutine coroutine;
        }
        private HoverState currentHover = null;

        private void Awake()
        {
            itemAssigner = GetComponent<ItemAssigner>();
            initialScale = transform.localScale;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isUsing) return;

            startPosition = transform.position;

            // 個数が残っているなら、元のオブジェクトはそのままでコピーを生成してドラッグする
            // 個数が1以下（つまりこれが最後の1個）なら、自分自身をドラッグする
            if (itemAssigner != null && itemAssigner.CurrentCount > 1)
            {
                CreateDragCopy();
            }
            else
            {
                draggingObject = gameObject;
                isDraggingCopy = false;

                if (dragCollider != null)
                {
                    dragCollider.enabled = false;
                }
            }
        }

        private void CreateDragCopy()
        {
            // 自分自身のコピーを作成
            draggingObject = Instantiate(gameObject, transform.parent);
            draggingObject.transform.position = transform.position;
            draggingObject.transform.localScale = transform.localScale;
            draggingObject.name = gameObject.name + "_DragCopy";

            // コピーには ItemUserコンポーネントは不要（ドラッグロジックはオリジナルが持つ）
            // ただし、見た目（SpriteRendererなど）と ItemAssigner（の見た目設定）は必要
            // コピーの ItemUser は無効化または削除する
            var copyUser = draggingObject.GetComponent<ItemUser>();
            if (copyUser != null) Destroy(copyUser);

            // コピーの ItemAssigner を取得し、CountTextを非表示にする
            var copyAssigner = draggingObject.GetComponent<ItemAssigner>();
            if (copyAssigner != null)
            {
                // コピーのAssignerに対して「個数表示を消す」処理を行いたいが、
                // ItemAssignerに専用メソッドがないため、ReflectionやFindで無理やりやるよりは
                // ItemAssigner側に対応を入れるのが筋だが、今回は簡易的にTransformから探すか
                // ItemAssignerのプロパティを操作できるならする。
                // ここでは、コピー作成直後なので、手動でCountTextを探して消す
                // ItemAssignerの構造上、countTextObjectがSerializeFieldされているはず
                
                // ItemAssignerスクリプトを変更せずにやるなら、
                // CopyのHierarchyからCountTextを探して非アクティブにする
                Transform countTextTrans = draggingObject.transform.Find("CountText"); // 名前決め打ちのリスク
                if (countTextTrans == null)
                {
                    // 名前が違うかもしれないので、TMPを探す手もあるが、
                    // ItemAssignerが持っている参照を使うのが確実だがprivate...
                    // しかしItemAssignerはStartでUpdateVisualsを呼ぶ。
                    // コピーのAssignerのCurrentCountは初期値(1)またはコピー元の値になる。
                    // ここでは単純に「コピー元のItemAssignerが参照しているCountTextオブジェクト」と同じ構造の子を探す
                    // あるいは、ItemAssigner.csを修正して CountText を public プロパティにするか、メソッドを追加する方が良い。
                    // 今回は ItemUser.cs の修正のみで対応するため、
                    // TMPコンポーネントを全検索して、"x"で始まるテキスト(CountText)を消す、等のヒューリスティックを使う
                    var tmps = draggingObject.GetComponentsInChildren<TMP_Text>();
                    foreach(var tmp in tmps)
                    {
                        if (tmp.text.StartsWith("x"))
                        {
                            tmp.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    countTextTrans.gameObject.SetActive(false);
                }
            }
            
            // コピーのColliderはRaycastに引っかからないように無効化しておく（ドラッグ中なので）
            var copyCollider = draggingObject.GetComponent<Collider2D>();
            if (copyCollider != null) copyCollider.enabled = false;
            var colliders = draggingObject.GetComponentsInChildren<Collider2D>();
            foreach(var c in colliders) c.enabled = false;

            isDraggingCopy = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isUsing || draggingObject == null) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
            mousePos.z = 0; 
            draggingObject.transform.position = mousePos;

            CheckHover(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isUsing) return;
            if (draggingObject == null) return;

            // Reset hover state on drop
            ResetCurrentHover();

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            bool used = false;

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
                        used = true;
                    }
                }
            }
            
            if (!used)
            {
                // キャンセル時
                if (isDraggingCopy)
                {
                    // コピーをドラッグしていた場合は、コピーを消すだけ
                    Destroy(draggingObject);
                }
                else
                {
                    // 本体をドラッグしていた場合は、元の位置に戻す
                    if (dragCollider != null)
                    {
                        dragCollider.enabled = true;
                    }
                    draggingObject.transform.position = startPosition;
                    // draggingObject.transform.localScale = initialScale; 
                }
            }
            
            // 参照を切る（UseItemAnimation内で使う場合は注意だが、今回は即座にローカル変数等で処理するか、draggingObjectを使う）
            if (!used)
            {
                draggingObject = null;
                isDraggingCopy = false;
            }
        }

        private void CheckHover(PointerEventData eventData)
        {
            // ホバー処理は同じ（Raycastを使用）
            // ... (変更なし)
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            Transform newTarget = null;

            if (hit.collider != null && hit.collider.tag.StartsWith("Worker"))
            {
                newTarget = hit.transform;
            }

            if (currentHover != null && currentHover.target != newTarget)
            {
                StartScaleAnimation(currentHover, currentHover.originalScale);
                currentHover = null;
            }

            if (newTarget != null && (currentHover == null || currentHover.target != newTarget))
            {
                currentHover = new HoverState
                {
                    target = newTarget,
                    originalScale = newTarget.localScale,
                };
                
                Vector3 targetScale = currentHover.originalScale * hoverScale;
                StartScaleAnimation(currentHover, targetScale);
            }
        }

        private void StartScaleAnimation(HoverState state, Vector3 targetScale)
        {
            if (state.coroutine != null) StopCoroutine(state.coroutine);
            state.coroutine = StartCoroutine(AnimateTargetScale(state, targetScale));
        }

        private IEnumerator AnimateTargetScale(HoverState state, Vector3 targetScale)
        {
            float elapsed = 0f;
            Vector3 startScale = state.target.localScale;

            while (elapsed < hoverAnimDuration)
            {
                if (state.target == null) yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hoverAnimDuration);
                t = Mathf.SmoothStep(0f, 1f, t); 
                
                state.target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            if (state.target != null)
            {
                state.target.localScale = targetScale;
            }
            state.coroutine = null;
        }

        private void ResetCurrentHover()
        {
            if (currentHover != null)
            {
                StartScaleAnimation(currentHover, currentHover.originalScale);
                currentHover = null;
            }
        }

        private IEnumerator UseItemAnimation(int workerIndex)
        {
            isUsing = true;
            
            // 効果適用（実際に減らすのはここではなく、アニメーション開始前か、確定時）
            ApplyItemToWorker(workerIndex);

            // アニメーション対象は draggingObject
            GameObject targetObj = draggingObject;

            // 0.1秒のアニメーション（アイテム自体のエフェクト）
            float duration = 0.1f;
            float halfDuration = duration / 2f;
            
            Vector3 startAnimScale = targetObj.transform.localScale;
            Vector3 midScale = startAnimScale * 1.2f; // 現在のスケールを基準に拡大
            Vector3 endScale = Vector3.zero;

            // 個数を減らす
            int remaining = itemAssigner.DecrementCount();

            if (isDraggingCopy)
            {
                // コピーを使っていた場合
                // 1. コピーは消滅アニメーションして消す
                // 2. 本体の個数表示は DecrementCount で更新されているはず

                // 消滅アニメーション
                float timer = 0f;
                while (timer < halfDuration)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / halfDuration);
                    targetObj.transform.localScale = Vector3.Lerp(startAnimScale, midScale, t);
                    yield return null;
                }
                timer = 0f;
                while (timer < halfDuration)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / halfDuration);
                    targetObj.transform.localScale = Vector3.Lerp(midScale, endScale, t);
                    yield return null;
                }
                Destroy(targetObj);

                // 本体は変更なし（位置もそのまま）
            }
            else
            {
                // 本体を使っていた場合（残り個数0になったはず）
                if (remaining > 0)
                {
                    // ここに来るのは論理的におかしい（残り1個だったから本体を使ったはずなので、remainingは0のはず）
                    // もし何かの間違いで残っているなら元の位置に戻す
                    targetObj.transform.position = startPosition;
                    if (dragCollider != null) dragCollider.enabled = true;
                }
                else
                {
                    // 消滅アニメーション
                    float timer = 0f;
                    while (timer < halfDuration)
                    {
                        timer += Time.deltaTime;
                        float t = Mathf.Clamp01(timer / halfDuration);
                        targetObj.transform.localScale = Vector3.Lerp(startAnimScale, midScale, t);
                        yield return null;
                    }
                    timer = 0f;
                    while (timer < halfDuration)
                    {
                        timer += Time.deltaTime;
                        float t = Mathf.Clamp01(timer / halfDuration);
                        targetObj.transform.localScale = Vector3.Lerp(midScale, endScale, t);
                        yield return null;
                    }
                    Destroy(targetObj);
                }
            }
            
            isUsing = false;
            draggingObject = null;
            isDraggingCopy = false;
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
