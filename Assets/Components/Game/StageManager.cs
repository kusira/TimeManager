using UnityEngine;
using TMPro;
using Components.Game.Graph.Scripts;
using Components.Game.Items.Scripts;
using Components.Game.Canvas.Scripts;

namespace Components.Game
{
    public class StageManager : MonoBehaviour
    {
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

        private void Awake()
        {
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
    }
}

