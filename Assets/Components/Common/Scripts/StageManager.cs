using UnityEngine;
using TMPro;
using Components.Game.Graph.Scripts;
using Components.Game.Items.Scripts;
using Components.Game.Canvas.Scripts;

namespace Components.Game
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("現在のステージインデックス")]
        [SerializeField] private int currentStageIndex = 0;

        [Header("UI References")]
        [Tooltip("ステージ番号を表示するテキスト")]
        [SerializeField] private TMP_Text stageText;

        [Header("External Components")]
        [SerializeField] private GraphGenerator graphGenerator;
        [SerializeField] private ItemGenerator itemGenerator;
        [SerializeField] private TimeLimitManager timeLimitManager;

        public int CurrentStageIndex => currentStageIndex;

        // 次のシーンロード時に適用するステージインデックス
        private static int? PendingStageIndex = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // PendingStageIndexがあれば適用
            if (PendingStageIndex.HasValue)
            {
                currentStageIndex = PendingStageIndex.Value;
                PendingStageIndex = null; // リセット
            }

            // 自動で探す (アサインされていなければ)
            if (graphGenerator == null) graphGenerator = FindFirstObjectByType<GraphGenerator>();
            if (itemGenerator == null) itemGenerator = FindFirstObjectByType<ItemGenerator>();
            if (timeLimitManager == null) timeLimitManager = FindFirstObjectByType<TimeLimitManager>();

            ApplyStageIndex();
            UpdateUI();
        }

        /// <summary>
        /// 各マネージャーに現在のステージインデックスを適用する
        /// </summary>
        public void ApplyStageIndex()
        {
            if (graphGenerator != null)
            {
                graphGenerator.SetStageIndex(currentStageIndex);
            }

            if (itemGenerator != null)
            {
                itemGenerator.SetStageIndex(currentStageIndex);
            }

            if (timeLimitManager != null)
            {
                timeLimitManager.SetStageIndex(currentStageIndex);
            }
        }

        /// <summary>
        /// UIを更新する
        /// </summary>
        private void UpdateUI()
        {
            if (stageText != null)
            {
                stageText.text = $"Stage {currentStageIndex + 1}";
            }
        }

        // 必要に応じて外部からステージを変更するメソッド
        public void SetStage(int index)
        {
            currentStageIndex = index;
            ApplyStageIndex();
            UpdateUI();
        }

        /// <summary>
        /// 次のステージを準備する（リロード後に適用される）
        /// </summary>
        public void PrepareNextStage()
        {
            PendingStageIndex = currentStageIndex + 1;
        }

        /// <summary>
        /// 次にロードされるシーンでのステージインデックスを指定する
        /// </summary>
        public static void SetNextStage(int index)
        {
            PendingStageIndex = index;
        }
    }
}

