using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // 追加
using Components.Game;

namespace Components.Game.Canvas.Scripts
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("背景オブジェクト (SetActive切り替え & CanvasGroup取得用)")]
        [SerializeField] private GameObject blackGround;
        [Tooltip("チュートリアルパネル (スライド用)")]
        [SerializeField] private Transform tutorialPanel;
        [Tooltip("閉じるボタン")]
        [SerializeField] private Button closeButton;

        [Header("Page Navigation")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI progressText;
        [Tooltip("ページとして表示するゲームオブジェクトのリスト (P1, P2, P3...)")]
        [SerializeField] private GameObject[] tutorialPages;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.3f; // MenuButtonManagerと同じ0.3fに合わせる
        [Tooltip("パネルのアニメーション移動距離")]
        [SerializeField] private float slideDistance = 50f;

        private Vector2 originalPosition;
        private CanvasGroup backgroundCanvasGroup;
        private RectTransform panelRectTransform;
        private CanvasGroup panelCanvasGroup;
        private int currentPageIndex = 0;

        void Start()
        {
            // ステージ0以外なら表示しない（即座に無効化）
            if (StageManager.Instance != null)
            {
                // ステージ0でない、または既に表示済みの場合は表示しない
                if (StageManager.Instance.CurrentStageIndex != 0 || StageManager.Instance.HasShownTutorial)
                {
                    gameObject.SetActive(false);
                    return;
                }
                
                // 表示フラグを立てる
                StageManager.Instance.HasShownTutorial = true;
            }

            // 初期化
            SetupUI();
            currentPageIndex = 0;
            UpdatePageUI(); // 初回のUI更新

            // 背景を表示
            if (blackGround != null) 
            {
                blackGround.SetActive(true);
                if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = 1f;
            }
            
            // パネル表示
            if (tutorialPanel != null)
            {
                if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1f;
                // 元の位置に戻す
                if (panelRectTransform != null) panelRectTransform.anchoredPosition = originalPosition;
            }

        }

        private void OnDestroy()
        {
            // ここでは Time.timeScale は触らない（別の仕組みで変更されている可能性があるため）
        }

        private void SetupUI()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseTutorial);
            
            if (prevButton != null) prevButton.onClick.AddListener(OnPrevPage);
            if (nextButton != null) nextButton.onClick.AddListener(OnNextPage);

            // 背景設定
            if (blackGround != null)
            {
                // CanvasGroup取得
                backgroundCanvasGroup = blackGround.GetComponent<CanvasGroup>();
                if (backgroundCanvasGroup == null)
                {
                    backgroundCanvasGroup = blackGround.AddComponent<CanvasGroup>();
                }
                backgroundCanvasGroup.alpha = 1f;

                // 背景クリックで閉じるボタン追加
                var btn = blackGround.GetComponent<Button>();
                if (btn == null)
                {
                    btn = blackGround.AddComponent<Button>();
                    btn.transition = Selectable.Transition.None;
                }
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(CloseTutorial);
            }

            // パネル設定
            if (tutorialPanel != null)
            {
                panelRectTransform = tutorialPanel as RectTransform;
                if (panelRectTransform == null) panelRectTransform = tutorialPanel.GetComponent<RectTransform>();

                if (panelRectTransform != null)
                {
                    originalPosition = panelRectTransform.anchoredPosition;
                }
                
                // パネルにもCanvasGroupがあれば取得
                panelCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = tutorialPanel.gameObject.AddComponent<CanvasGroup>();
                }
                panelCanvasGroup.alpha = 1f;
                
                // パネル自体のクリックが背景に抜けないようにダミーボタンを追加
                var panelSelectable = tutorialPanel.GetComponent<Selectable>();
                if (panelSelectable == null)
                {
                     var dummyBtn = tutorialPanel.gameObject.AddComponent<Button>();
                     dummyBtn.transition = Selectable.Transition.None;
                }
                
                // 初期位置設定はここでは行わず、Startでの表示時に行う（アニメーションしないため）
            }
        }

        public void CloseTutorial()
        {
            if (blackGround != null) blackGround.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnPrevPage()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdatePageUI();
            }
        }

        private void OnNextPage()
        {
            if (tutorialPages != null && currentPageIndex < tutorialPages.Length - 1)
            {
                currentPageIndex++;
                UpdatePageUI();
            }
        }

        private void UpdatePageUI()
        {
            if (tutorialPages == null) return;

            int totalPages = tutorialPages.Length;

            // ページの表示切り替え
            for (int i = 0; i < totalPages; i++)
            {
                if (tutorialPages[i] != null)
                {
                    tutorialPages[i].SetActive(i == currentPageIndex);
                }
            }

            // テキスト更新
            if (progressText != null)
            {
                progressText.text = $"{currentPageIndex + 1} / {totalPages}";
            }

            // ボタンの表示/非表示
            if (prevButton != null)
            {
                prevButton.gameObject.SetActive(currentPageIndex > 0);
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(currentPageIndex < totalPages - 1);
            }
        }

        // アニメーション処理は削除
        // private IEnumerator AnimateTutorial(bool opening) { ... }
    }
}
