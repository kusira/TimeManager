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
            public List<string> itemIds = new List<string>();
        }

        [SerializeField] private List<StageData> stages = new List<StageData>();

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
