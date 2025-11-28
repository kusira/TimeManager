using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 追加

namespace Components.Game.Canvas.Scripts
{
    public class TimeLimitManager : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("時間経過を表すゲージ (Image component)")]
        [SerializeField] private Image gaugeImage;
        
        [Tooltip("経過時間/制限時間を表示するテキスト")]
        [SerializeField] private TextMeshProUGUI timeLimitText;

        [Header("Stage Settings")]
        [Tooltip("ステージデータベース")]
        [SerializeField] private StageDatabase stageDatabase;
        [Tooltip("現在のステージインデックス")]
        [SerializeField] private int currentStageIndex = 0;

        public void SetStageIndex(int index)
        {
            currentStageIndex = index;
            // ステージが変わったらデータを再ロードしてリセットなどが必要
            LoadStageData();
            // InitializeGauge(); // 初期幅が0で上書きされるのを防ぐため削除（Awakeで取得済み）
            StartTimer();
        }

        [Header("Game Status")]
        [SerializeField] private bool isRunning = false;
        
        private float timeLimit = 60f;
        private float currentTime = 0f;
        private float initialWidth;
        private RectTransform gaugeRect;

        // 外部から時間超過を知るためのイベント（必要なら使用）
        // public event System.Action OnTimeUp;

        [SerializeField] private ResultManager resultManager;
        [SerializeField] private Components.Game.Graph.Scripts.TaskProgresser taskProgresser;

        private void Awake()
        {
            InitializeGauge();
        }

        private void Start()
        {
            if (resultManager == null) resultManager = FindFirstObjectByType<ResultManager>();
            if (taskProgresser == null) taskProgresser = FindFirstObjectByType<Components.Game.Graph.Scripts.TaskProgresser>();

            // Awakeで初期化済みなのでここでは不要、ただし未初期化なら実行
            if (initialWidth <= 0) InitializeGauge();

            LoadStageData();
            
            // 自動スタート (必要に応じて変更)
            StartTimer();
        }

        private void Update()
        {
            if (!isRunning) return;

            currentTime += Time.deltaTime;

            UpdateGaugeVisuals();

            if (currentTime >= timeLimit)
            {
                currentTime = timeLimit;
                isRunning = false;
                OnTimeLimitExceeded();
            }
        }

        private void InitializeGauge()
        {
            // 既に取得済みなら何もしない
            if (initialWidth > 0) return;

            if (gaugeImage != null)
            {
                gaugeRect = gaugeImage.GetComponent<RectTransform>();
                if (gaugeRect != null)
                {
                    initialWidth = gaugeRect.rect.width;
                }
            }
        }

        private void LoadStageData()
        {
            if (stageDatabase != null)
            {
                var data = stageDatabase.GetStageData(currentStageIndex);
                if (data != null)
                {
                    timeLimit = data.timeLimit;
                }
            }
        }

        public void StartTimer()
        {
            currentTime = 0f;
            isRunning = true;
            UpdateGaugeVisuals();
        }

        public void StopTimer()
        {
            isRunning = false;
        }

        private void UpdateGaugeVisuals()
        {
            if (gaugeRect == null) return;

            float progress = Mathf.Clamp01(currentTime / timeLimit);
            float currentWidth = initialWidth * progress;

            // 高さは変えず、幅だけ変更
            gaugeRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
            
            // テキスト更新
            if (timeLimitText != null)
            {
                // <現在時間(少数1桁)> / <制限時間(整数)> s
                timeLimitText.text = $"{currentTime:F1} / {timeLimit:F0} s";
            }
        }

        private void OnTimeLimitExceeded()
        {
            Debug.Log("Time Limit Exceeded!");
            
            // タスクの進行を停止
            if (taskProgresser != null)
            {
                taskProgresser.StopProgress();
                taskProgresser.OnGameOverResultStarted();
            }
            
            if (resultManager != null)
            {
                resultManager.ShowGameOver();
            }
        }
    }
}

