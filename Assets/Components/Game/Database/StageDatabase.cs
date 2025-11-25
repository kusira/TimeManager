using System.Collections.Generic;
using UnityEngine;
using Components.Game.Graph.Scripts; // GraphGeneratorの定義を使用

namespace Components.Game
{
    [CreateAssetMenu(fileName = "StageDatabase", menuName = "Game/StageDatabase")]
    public class StageDatabase : ScriptableObject
    {
        [System.Serializable]
        public class StageData
        {
            public string stageName;
            
            [Header("Graph Data")]
            public List<GraphGenerator.VertexData> vertices = new List<GraphGenerator.VertexData>();
            public List<GraphGenerator.EdgeData> edges = new List<GraphGenerator.EdgeData>();
            
            [Header("Item Data")]
            // public List<string> itemIds = new List<string>();
            public List<StageItemData> stageItems = new List<StageItemData>();

            [Header("Time Settings")]
            [Tooltip("制限時間 (秒)")]
            public float timeLimit = 60f;

            [Header("Worker Settings")]
            [Tooltip("初期配置するワーカーの人数")]
            public int initialWorkerCount = 1;
        }

        [System.Serializable]
        public class StageItemData
        {
            public string itemId;
            public int count = 1;
        }

        [SerializeField] private List<StageData> stages = new List<StageData>();

        public int StageCount => stages.Count;

        /// <summary>
        /// 指定されたインデックスのステージデータを取得します。
        /// </summary>
        public StageData GetStageData(int index)
        {
            if (index >= 0 && index < stages.Count)
            {
                return stages[index];
            }
            Debug.LogWarning($"Stage index {index} is out of range. returning null.");
            return null;
        }
    }
}
