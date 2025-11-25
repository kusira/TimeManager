using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Ending.Scripts
{
    public class EndingAppearer : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("フェードインさせる対象のオブジェクト")]
        [SerializeField] private GameObject targetObject;

        [Header("Animation Settings")]
        [Tooltip("フェードインにかかる時間")]
        [SerializeField] private float fadeDuration = 1.0f;
        [Tooltip("開始までの遅延時間")]
        [SerializeField] private float startDelay = 0.5f;
        [Tooltip("下から出現する際の移動距離")]
        [SerializeField] private float slideDistance = 50f;

        private CanvasGroup targetCanvasGroup;
        private Vector3 targetPosition;
        private Vector3 startPosition;

        private void Awake()
        {
            if (targetObject != null)
            {
                // CanvasGroupの取得または追加
                targetCanvasGroup = targetObject.GetComponent<CanvasGroup>();
                if (targetCanvasGroup == null)
                {
                    targetCanvasGroup = targetObject.AddComponent<CanvasGroup>();
                }

                // 初期位置と目標位置の設定
                // 現在の位置を目標位置（最終地点）とする
                targetPosition = targetObject.transform.localPosition;
                // 下にずらした位置を開始位置とする
                startPosition = targetPosition - new Vector3(0, slideDistance, 0);

                // 初期状態の適用（透明＆下に配置）
                targetCanvasGroup.alpha = 0f;
                targetObject.transform.localPosition = startPosition;
                
                // 念のためアクティブにする（非アクティブだとコルーチンが動かない場合があるが、
                // このスクリプトが別オブジェクトにあるなら大丈夫。ターゲット自身のスクリプトなら注意）
                targetObject.SetActive(true);
            }
        }

        private void Start()
        {
            if (targetObject != null)
            {
                StartCoroutine(AnimateAppear());
            }
        }

        private IEnumerator AnimateAppear()
        {
            if (startDelay > 0)
            {
                yield return new WaitForSeconds(startDelay);
            }

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeDuration);
                
                // EaseOutQuad (ResultManagerと同じイージング)
                float easeT = 1f - (1f - t) * (1f - t); 

                if (targetCanvasGroup != null)
                {
                    targetCanvasGroup.alpha = t;
                    targetObject.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, easeT);
                }

                yield return null;
            }

            // 最終状態を確実に適用
            if (targetCanvasGroup != null)
            {
                targetCanvasGroup.alpha = 1f;
                targetObject.transform.localPosition = targetPosition;
            }
        }
    }
}

