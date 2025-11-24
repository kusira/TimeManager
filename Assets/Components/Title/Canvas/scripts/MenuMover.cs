using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace Components.Title.Canvas.Scripts
{
    public class MenuMover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float moveDistance = 20f;
        [SerializeField] private float duration = 0.3f;

        private Vector3 initialPosition;
        private Coroutine currentCoroutine;
        private RectTransform rectTransform;
        private bool isInitialized = false;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            // レイアウトグループ等の影響を考慮し、Startで初期位置を取得
            // 必要に応じて遅延初期化も検討
            initialPosition = rectTransform.anchoredPosition;
            isInitialized = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInitialized) return;
            
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            // 左へ移動 (-x方向)
            Vector3 targetPos = initialPosition - new Vector3(moveDistance, 0f, 0f);
            currentCoroutine = StartCoroutine(AnimateMove(targetPos));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInitialized) return;

            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            // 元の位置に戻る
            currentCoroutine = StartCoroutine(AnimateMove(initialPosition));
        }

        private IEnumerator AnimateMove(Vector3 targetPos)
        {
            float elapsed = 0f;
            Vector3 startPos = rectTransform.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // イージング (SmoothStep)
                t = Mathf.SmoothStep(0f, 1f, t);

                rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPos;
            currentCoroutine = null;
        }
    }
}

