using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Game.Canvas.Scripts
{
    public class TimeLimitManager : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("時間経過を表すゲージ (Image component)")]
        [SerializeField] private Image gaugeImage;

        [Header("Stage Settings")]
        [Tooltip("ステージデータベース")]
        [SerializeField] private StageDatabase stageDatabase;
        [Tooltip("現在のステージインデックス")]
        [SerializeField] private int currentStageIndex = 0;

        [Header("Game Status")]
        [SerializeField] private bool isRunning = false;
        
        private float timeLimit = 60f;
        private float currentTime = 0f;
        private float initialWidth;
        private RectTransform gaugeRect;

        // 外部から時間超過を知るためのイベント（必要なら使用）
        // public event System.Action OnTimeUp;

        private void Start()
        {
            InitializeGauge();
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
            if (gaugeImage != null)
            {
                gaugeRect = gaugeImage.GetComponent<RectTransform>();
                if (gaugeRect != null)
                {
                    initialWidth = gaugeRect.rect.width;
                }
                // 初期状態は0からスタートするのでwidthを0にするか、scaleを使うか
                // ここではwidthを変更する方式（Simple FillなどではなくRectTransformのサイズ変更）を想定
                // ただし、「現在最大値のwidthをとっています」とのことなので、
                // 0秒でwidth=0, 制限時間でwidth=initialWidth になるようにします。
                
                // Image TypeがFilledの場合は fillAmount を使うのが一般的ですが、
                // widthを指定されたので sizeDelta を操作します。
                // アンカー設定によっては挙動が変わるため注意が必要。
                // 左詰めにするには Pivot X=0 であることが望ましい。
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
        }

        private void OnTimeLimitExceeded()
        {
            Debug.Log("Time Limit Exceeded!");
            // ゲームオーバー処理などをここに記述、またはイベント発火
            // OnTimeUp?.Invoke();
        }
    }
}

