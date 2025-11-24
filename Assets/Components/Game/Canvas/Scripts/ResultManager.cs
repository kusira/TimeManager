using System.Collections;
using System.Collections.Generic; // 追加
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Components.Game.Canvas.Scripts
{
    public class ResultManager : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("リザルト表示時の背景 (Image/CanvasGroup)")]
        [SerializeField] private Image blackGround;
        [Tooltip("結果テキストを表示する画像 (GameClear/GameOver)")]
        [SerializeField] private Image resultTextImage;
        [Tooltip("ボタンを配置する親トランスフォーム")]
        [SerializeField] private Transform resultButtonsParent;
        
        [Header("External Managers")]
        [SerializeField] private FadeManager fadeManager;
        [SerializeField] private StageManager stageManager;

        [Header("Sprites")]
        [Tooltip("クリア時のテキスト画像")]
        [SerializeField] private Sprite gameClearSprite;
        [Tooltip("ゲームオーバー時のテキスト画像")]
        [SerializeField] private Sprite gameOverSprite;
        [Tooltip("Homeボタンの画像")]
        [SerializeField] private Sprite homeButtonSprite;
        [Tooltip("Replayボタンの画像")]
        [SerializeField] private Sprite replayButtonSprite;
        [Tooltip("Nextボタンの画像")]
        [SerializeField] private Sprite nextButtonSprite;

        [Header("Prefabs")]
        [Tooltip("リザルトボタンのPrefab")]
        [SerializeField] private GameObject resultButtonPrefab;

        [Header("Settings")]
        [Tooltip("ホームシーンの名前")]
        [SerializeField] private string homeSceneName = "TitleScene";
        [Tooltip("ボタン間のX軸間隔")]
        [SerializeField] private float buttonSpacingX = 150f;

        [Header("Animation Settings")]
        [Tooltip("リザルト表示フェード時間")]
        [SerializeField] private float fadeDuration = 0.5f;
        [Tooltip("UIパーツ出現時の遅延設定 (0: Delay, 1: Delay*2 ...)")]
        [SerializeField] private float UIDelayStep = 0.3f;
        [Tooltip("UIパーツ出現時のY軸移動距離")]
        [SerializeField] private float UISlideDistance = 50f;

        [System.Serializable]
        public class UIAnimationGroup
        {
            public string groupName;
            public List<CanvasGroup> UIElements;
        }

        [Header("UI Groups (Animation Order)")]
        [Tooltip("アニメーション順序グループ (0から順に表示)")]
        [SerializeField] private System.Collections.Generic.List<UIAnimationGroup> uiGroups = new System.Collections.Generic.List<UIAnimationGroup>();

        private CanvasGroup backgroundCanvasGroup;

        private void Awake()
        {
            if (blackGround != null)
            {
                backgroundCanvasGroup = blackGround.GetComponent<CanvasGroup>();
                if (backgroundCanvasGroup == null)
                {
                    backgroundCanvasGroup = blackGround.gameObject.AddComponent<CanvasGroup>();
                }
                backgroundCanvasGroup.alpha = 0f;
                blackGround.gameObject.SetActive(false);
            }

            if (resultTextImage != null)
            {
                resultTextImage.gameObject.SetActive(false);
            }

            if (fadeManager == null) fadeManager = FindFirstObjectByType<FadeManager>();
            if (stageManager == null) stageManager = FindFirstObjectByType<StageManager>();
        }

        public void ShowGameClear()
        {
            StartCoroutine(ShowResultSequence(true));
        }

        public void ShowGameOver()
        {
            StartCoroutine(ShowResultSequence(false));
        }

        private IEnumerator ShowResultSequence(bool isClear)
        {
            // 時間停止は行わない（TimeLimitManagerでのみ停止）
            // Time.timeScale = 0f;

            // 1. 背景フェードイン
            if (blackGround != null && backgroundCanvasGroup != null)
            {
                blackGround.gameObject.SetActive(true);
                float timer = 0f;
                while (timer < fadeDuration)
                {
                    timer += Time.unscaledDeltaTime;
                    backgroundCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                    yield return null;
                }
                backgroundCanvasGroup.alpha = 1f;
            }

            // 2. UIグループアニメーション準備
            if (resultTextImage != null)
            {
                resultTextImage.sprite = isClear ? gameClearSprite : gameOverSprite;
                resultTextImage.gameObject.SetActive(true);
                resultTextImage.SetNativeSize();
            }
            GenerateButtons(isClear);

            // 初期状態設定（透明 & 位置オフセット）
            foreach (var group in uiGroups)
            {
                foreach (var cg in group.UIElements)
                {
                    if (cg != null)
                    {
                        cg.alpha = 0f;
                        cg.gameObject.SetActive(true); // 表示状態にしておく
                        cg.transform.localPosition -= new Vector3(0, UISlideDistance, 0);
                    }
                }
            }

            // 3. 順次フェードイン
            for (int i = 0; i < uiGroups.Count; i++)
            {
                // グループごとの遅延
                if (i > 0)
                {
                    yield return new WaitForSecondsRealtime(UIDelayStep);
                }

                StartCoroutine(AnimateUIGroup(uiGroups[i]));
            }
        }

        private IEnumerator AnimateUIGroup(UIAnimationGroup group)
        {
            float timer = 0f;
            // 初期位置を記録（現在の位置はすでにオフセットされている）
            // ただし、複数要素あるので個別に処理
            var startPositions = new List<Vector3>();
            var targetPositions = new List<Vector3>();

            foreach (var cg in group.UIElements)
            {
                if (cg != null)
                {
                    startPositions.Add(cg.transform.localPosition);
                    targetPositions.Add(cg.transform.localPosition + new Vector3(0, UISlideDistance, 0));
                }
                else
                {
                    startPositions.Add(Vector3.zero);
                    targetPositions.Add(Vector3.zero);
                }
            }

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / fadeDuration);
                float easeT = 1f - (1f - t) * (1f - t); // EaseOutQuad

                for (int j = 0; j < group.UIElements.Count; j++)
                {
                    var cg = group.UIElements[j];
                    if (cg != null)
                    {
                        cg.alpha = t;
                        cg.transform.localPosition = Vector3.Lerp(startPositions[j], targetPositions[j], easeT);
                    }
                }
                yield return null;
            }

            // 最終状態
            for (int j = 0; j < group.UIElements.Count; j++)
            {
                var cg = group.UIElements[j];
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.transform.localPosition = targetPositions[j];
                }
            }
        }

        private void GenerateButtons(bool isClear)
        {
            if (resultButtonPrefab == null || resultButtonsParent == null) return;

            // 既存のボタンをクリア
            foreach (Transform child in resultButtonsParent)
            {
                Destroy(child.gameObject);
            }

            // ボタンの定義 (タイプ, スプライト, アクション)
            // ラムダ式を明示的に UnityEngine.Events.UnityAction にキャストするか、型を明示する
            var buttons = new System.Collections.Generic.List<(Sprite, UnityEngine.Events.UnityAction)>();

            UnityEngine.Events.UnityAction homeAction = () => 
            {
                Time.timeScale = 1f;
                if (fadeManager != null) fadeManager.FadeOutAndLoadScene(homeSceneName);
                else SceneManager.LoadScene(homeSceneName);
            };
            buttons.Add((homeButtonSprite, homeAction));

            UnityEngine.Events.UnityAction replayAction = () => 
            {
                Time.timeScale = 1f;

                // リプレイ時は現在のステージインデックスを維持する
                if (stageManager != null)
                {
                    StageManager.SetNextStage(stageManager.CurrentStageIndex);
                }

                string currentScene = SceneManager.GetActiveScene().name;
                if (fadeManager != null) fadeManager.FadeOutAndLoadScene(currentScene);
                else SceneManager.LoadScene(currentScene);
            };
            buttons.Add((replayButtonSprite, replayAction));

            // Next (クリア時のみ)
            if (isClear)
            {
                UnityEngine.Events.UnityAction nextAction = () => 
                {
                    Time.timeScale = 1f;
                    if (stageManager != null) stageManager.PrepareNextStage();
                    
                    string currentScene = SceneManager.GetActiveScene().name;
                    if (fadeManager != null) fadeManager.FadeOutAndLoadScene(currentScene);
                    else SceneManager.LoadScene(currentScene);
                };
                buttons.Add((nextButtonSprite, nextAction));
            }

            // ボタン生成と配置
            float startX = -((buttons.Count - 1) * buttonSpacingX) / 2f;

            for (int i = 0; i < buttons.Count; i++)
            {
                GameObject btnObj = Instantiate(resultButtonPrefab, resultButtonsParent);
                btnObj.transform.localPosition = new Vector3(startX + (i * buttonSpacingX), 0, 0);
                btnObj.transform.localScale = Vector3.one; // スケールリセット

                // 画像設定
                Image btnImage = btnObj.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.sprite = buttons[i].Item1;
                    // btnImage.SetNativeSize(); // 必要に応じて
                }

                // クリックイベント設定
                Button btnComp = btnObj.GetComponent<Button>();
                if (btnComp == null) btnComp = btnObj.AddComponent<Button>();
                
                btnComp.onClick.RemoveAllListeners();
                btnComp.onClick.AddListener(buttons[i].Item2);
            }
        }
    }
}

