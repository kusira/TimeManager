using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Components.Game.Canvas.Scripts
{
    public class FadeManager : MonoBehaviour
    {
        [Header("UI参照")]
        [Tooltip("フェードに使用する画像 (全画面を覆う黒い画像など)")]
        [SerializeField] private Image fadeImage;

        [Header("設定")]
        [Tooltip("シーン開始時にフェードインを行うか")]
        [SerializeField] private bool fadeInOnStart = true;
        [Tooltip("シーン開始時のフェードイン開始までの遅延時間 (秒)")]
        [SerializeField] private float startDelay = 0f;
        [Tooltip("フェードアニメーションの時間 (秒)")]
        [SerializeField] private float animationDuration = 0.3f;

        private void Start()
        {
            if (fadeImage != null)
            {
                // シーン開始時のフェードイン設定
                if (fadeInOnStart)
                {
                    // 最初は真っ暗にしてからフェードイン
                    SetAlpha(1f);
                    StartCoroutine(FadeInCoroutine(startDelay));
                }
                else
                {
                    // フェードインしない場合は透明にして非アクティブに
                    SetAlpha(0f);
                    fadeImage.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// フェードインを行う (暗転 -> 透明)
        /// </summary>
        /// <param name="delay">開始までの遅延時間</param>
        public void FadeIn(float delay = 0f)
        {
            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
                StartCoroutine(FadeInCoroutine(delay));
            }
        }

        /// <summary>
        /// フェードアウトを行い、完了後に指定したシーンへ遷移する (透明 -> 暗転 -> 遷移)
        /// </summary>
        /// <param name="sceneName">遷移先のシーン名</param>
        /// <param name="delay">開始までの遅延時間</param>
        public void FadeOutAndLoadScene(string sceneName, float delay = 0f)
        {
            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
                StartCoroutine(FadeOutAndLoadSceneCoroutine(sceneName, delay));
            }
            else
            {
                // Imageがない場合は即遷移
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator FadeInCoroutine(float delay)
        {
            SetAlpha(1f);
            
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            float timer = 0f;
            while (timer < animationDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / animationDuration);
                // EaseOut
                float alpha = 1f - t;
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(0f);
            if (fadeImage != null) fadeImage.gameObject.SetActive(false);
        }

        private IEnumerator FadeOutAndLoadSceneCoroutine(string sceneName, float delay)
        {
            SetAlpha(0f);

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            float timer = 0f;
            while (timer < animationDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / animationDuration);
                // EaseIn
                float alpha = t;
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(1f);
            
            // 遷移
            SceneManager.LoadScene(sceneName);
        }

        private void SetAlpha(float alpha)
        {
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
            }
        }
    }
}

