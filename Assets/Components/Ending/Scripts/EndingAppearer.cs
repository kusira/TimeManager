using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Ending.Scripts
{
    public class EndingAppearer : MonoBehaviour
    {
        // インスペクターで設定するためのクラス定義
        [System.Serializable]
        public class FadeTarget
        {
            [Tooltip("フェードインさせる対象のオブジェクト")]
            public GameObject targetObject;
            
            [Tooltip("このオブジェクトのフェードインにかかる時間（秒）")]
            public float duration = 1.0f;
        }

        [Header("Targets")]
        [Tooltip("フェードインさせる対象と時間のリスト（上から順に表示されます）")]
        [SerializeField] private List<FadeTarget> targetList = new List<FadeTarget>();

        [Header("Sequence Settings")]
        [Tooltip("最初のアニメーション開始までの遅延時間")]
        [SerializeField] private float startDelay = 0.5f;
        [Tooltip("次のオブジェクトが表示開始されるまでの遅延時間")]
        [SerializeField] private float elementDelay = 0.3f;

        // 内部で保持する実行用データ
        private class RuntimeTargetData
        {
            public GameObject obj;
            public CanvasGroup cg;
            public float duration;
        }

        private List<RuntimeTargetData> initializedTargets = new List<RuntimeTargetData>();

        private void Awake()
        {
            foreach (var item in targetList)
            {
                if (item.targetObject == null) continue;

                var data = new RuntimeTargetData();
                data.obj = item.targetObject;
                data.duration = item.duration; // 個別の時間を保持

                // CanvasGroupの取得または追加
                data.cg = item.targetObject.GetComponent<CanvasGroup>();
                if (data.cg == null)
                {
                    data.cg = item.targetObject.AddComponent<CanvasGroup>();
                }

                // 初期状態の適用（透明にするだけ。位置は変更しない）
                data.cg.alpha = 0f;
                
                // 念のためアクティブにする
                item.targetObject.SetActive(true);

                initializedTargets.Add(data);
            }
        }

        private void Start()
        {
            if (initializedTargets.Count > 0)
            {
                StartCoroutine(AnimateSequence());
            }
        }

        private IEnumerator AnimateSequence()
        {
            // 最初の開始遅延
            if (startDelay > 0)
            {
                yield return new WaitForSeconds(startDelay);
            }

            for (int i = 0; i < initializedTargets.Count; i++)
            {
                // 2つ目以降は elementDelay 分待つ
                if (i > 0 && elementDelay > 0)
                {
                    yield return new WaitForSeconds(elementDelay);
                }

                StartCoroutine(AnimateSingle(initializedTargets[i]));
            }
        }

        private IEnumerator AnimateSingle(RuntimeTargetData data)
        {
            float timer = 0f;
            // 個別に設定された duration を使用する
            float currentDuration = data.duration; 
            
            // durationが0以下の場合は即時表示して終了
            if (currentDuration <= 0f)
            {
                if (data.cg != null) data.cg.alpha = 1f;
                yield break;
            }
            
            while (timer < currentDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / currentDuration);
                
                // EaseOutQuad (フェードインが自然に見えるイージング)
                // 好みに応じて t そのままでもOKです
                // float easeT = t; 
                float easeT = 1f - (1f - t) * (1f - t); 

                if (data.obj != null && data.cg != null)
                {
                    data.cg.alpha = easeT;
                    // 位置の変更処理は削除しました
                }

                yield return null;
            }

            // 最終状態を確実に適用
            if (data.obj != null && data.cg != null)
            {
                data.cg.alpha = 1f;
            }
        }
    }
}