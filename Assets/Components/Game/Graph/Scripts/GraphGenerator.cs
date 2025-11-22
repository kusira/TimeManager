using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Components.Game.Graph.Scripts
{
    // WorkerType Enum removed as it's determined by row index 'i'

    public class GraphGenerator : MonoBehaviour
    {
        [System.Serializable]
        public class VertexData
        {
            [Tooltip("縦方向のインデックス (行)")]
            public int i; 
            [Tooltip("横方向のインデックス (列)")]
            public int j;
            public float taskCompletionTime;
            // workerLabel removed
        }

        [System.Serializable]
        public class EdgeData
        {
            public int fromIndex;
            public int toIndex;
        }

        [Header("Global Settings")]
        [Tooltip("横方向の間隔 (J軸)")]
        [SerializeField] private float gridSpacingJ = 2.0f;
        [Tooltip("縦方向の間隔 (I軸)")]
        [SerializeField] private float gridSpacingI = 2.0f;
        [Tooltip("辺の長さの倍率")]
        [SerializeField] private float edgeLengthMultiplier = 0.7f;
        [SerializeField] private GameObject vertexPrefab;
        [SerializeField] private GameObject edgePrefab;
        [SerializeField] private Transform graphParent;

        [Header("Graph Data")]
        [SerializeField] private Components.Game.StageDatabase stageDatabase;
        [SerializeField] private int currentStageIndex = 0;
        
        // インスペクタからの直接入力を防ぐため SerializeField を削除
        private List<VertexData> vertices = new List<VertexData>();
        private List<EdgeData> edges = new List<EdgeData>();

        // 生成された頂点のGameObjectを保持するリスト（外部参照用）
        private List<GameObject> generatedVertexObjects = new List<GameObject>();

        private void Start()
        {
            // StageDatabaseからデータをロードする
            LoadStageData();
            GenerateGraph();
        }

        private void LoadStageData()
        {
            if (stageDatabase != null)
            {
                var stageData = stageDatabase.GetStageData(currentStageIndex);
                if (stageData != null)
                {
                    this.vertices = new List<VertexData>(stageData.vertices);
                    this.edges = new List<EdgeData>(stageData.edges);
                    Debug.Log($"Loaded Stage Data: {stageData.stageName} (Vertices: {vertices.Count}, Edges: {edges.Count})");
                }
            }
        }

        public void GenerateGraph()
        {
            if (graphParent == null)
            {
                Debug.LogError("Graph Parent is not assigned!");
                return;
            }

            // 既存の子オブジェクトをクリア
            var children = new List<GameObject>();
            foreach (Transform child in graphParent)
            {
                children.Add(child.gameObject);
            }
            foreach (var child in children)
            {
                DestroyImmediate(child);
            }

            // リストクリア
            generatedVertexObjects.Clear();

            // 辺を接続するために生成された頂点のTransformを保存
            var generatedVertices = new Dictionary<int, Transform>();

            // 頂点の生成
            for (int i = 0; i < vertices.Count; i++)
            {
                var vertexData = vertices[i];
                if (vertexPrefab != null)
                {
                    // 座標計算: (0,0) が左上
                    Vector3 position = new Vector3(
                        vertexData.j * gridSpacingJ, 
                        -vertexData.i * gridSpacingI, 
                        0
                    );

                    GameObject vertexObj = Instantiate(vertexPrefab, graphParent);
                    vertexObj.transform.localPosition = position;
                    // Workerは i + 1 番目の文字 (1->A, 2->B...)
                    // Assuming i=0 -> 1st row -> A.
                    char workerChar = (char)('A' + vertexData.i); 
                    
                    vertexObj.name = $"Vertex_{i} (Time: {vertexData.taskCompletionTime}, Worker: {workerChar})";

                    // VertexPrefab直下の "IndexText" という名前の子オブジェクトを探し、
                    // そのコンポーネントまたはその子にある TMP_Text に頂点番号を設定する
                    Transform indexTransform = vertexObj.transform.Find("IndexText");
                    if (indexTransform != null)
                    {
                        var tmpText = indexTransform.GetComponent<TMP_Text>();
                        // もし直下になければ Index オブジェクトの子も探すか、あるいは単にGetComponentで探す
                        // リクエスト: "IndexというゲームオブジェクトにアタッチされているTextMeshPro"
                        if (tmpText != null)
                        {
                            tmpText.text = (i + 1).ToString();
                        }
                        else
                        {
                            // IndexオブジェクトはあるがTMPが直下にない場合、念のためその子も探すか、ログを出す
                            // 今回はIndexオブジェクト自体にアタッチされていると想定
                        }
                    }
                    
                    generatedVertices.Add(i, vertexObj.transform);
                    generatedVertexObjects.Add(vertexObj);
                }
            }

            // 辺の生成
            if (edgePrefab != null)
            {
                foreach (var edgeData in edges)
                {
                    if (generatedVertices.ContainsKey(edgeData.fromIndex) && generatedVertices.ContainsKey(edgeData.toIndex))
                    {
                        Transform startT = generatedVertices[edgeData.fromIndex];
                        Transform endT = generatedVertices[edgeData.toIndex];

                        Vector3 startPos = startT.localPosition;
                        Vector3 endPos = endT.localPosition;
                        
                        // 中点に配置
                        Vector3 midPoint = (startPos + endPos) / 2f;
                        GameObject edgeObj = Instantiate(edgePrefab, graphParent);
                        edgeObj.transform.localPosition = midPoint;
                        edgeObj.name = $"Edge_{edgeData.fromIndex}_{edgeData.toIndex}";

                        // 回転
                        Vector3 direction = endPos - startPos;
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        edgeObj.transform.localRotation = Quaternion.Euler(0, 0, angle);

                        // スケール
                        float distance = Vector3.Distance(startPos, endPos);
                        Vector3 scale = edgeObj.transform.localScale;
                        scale.x = distance * edgeLengthMultiplier;
                        edgeObj.transform.localScale = scale;
                    }
                    else
                    {
                        Debug.LogWarning($"無効な辺のインデックスです: {edgeData.fromIndex} -> {edgeData.toIndex}");
                    }
                }
            }
        }

        /// <summary>
        /// グラフの頂点データを取得します。
        /// </summary>
        public List<VertexData> GetVertices()
        {
            return vertices;
        }

        /// <summary>
        /// グラフの辺データを取得します。
        /// </summary>
        public List<EdgeData> GetEdges()
        {
            return edges;
        }

        /// <summary>
        /// 頂点と辺の全データを取得します。
        /// </summary>
        public (List<VertexData>, List<EdgeData>) GetGraphData()
        {
            return (vertices, edges);
        }

        /// <summary>
        /// 生成された頂点のGameObjectリストを取得します。
        /// </summary>
        public List<GameObject> GetGeneratedVertexObjects()
        {
            return generatedVertexObjects;
        }
        
        /// <summary>
        /// I軸インデックスからWorkerタイプ（インデックス）を取得
        /// 0 -> A, 1 -> B, 2 -> C ...
        /// </summary>
        public int GetWorkerIndexFromRow(int i)
        {
            // 0-based index corresponding to A, B, C...
            return i; 
        }
    }
}
