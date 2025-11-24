using System.Collections.Generic;
using UnityEngine;

namespace Components.Game.Workers.Scripts
{
    public class WorkersGenerator : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("ワーカーのPrefabリスト (A, B, C...)")]
        [SerializeField] private List<GameObject> workerPrefabs;

        [Tooltip("ワーカー間のY軸の間隔")]
        [SerializeField] private float verticalSpacing = 3.8f;

        [Tooltip("人数ごとのスケール設定 (インデックス0=1人, 1=2人...)")]
        [SerializeField] private List<float> scaleSettings = new List<float>();

        [Header("References")]
        [Tooltip("ステージデータベース")]
        [SerializeField] private StageDatabase stageDatabase;
        [Tooltip("現在のステージ管理")]
        [SerializeField] private StageManager stageManager;

        // 生成されたワーカーのリストを公開
        public List<GameObject> GeneratedWorkers { get; private set; } = new List<GameObject>();

        private void Start()
        {
            // StageManagerがアサインされていない場合は探す
            if (stageManager == null)
            {
                stageManager = StageManager.Instance;
            }

            // ワーカー生成
            GenerateWorkers();
        }

        public void GenerateWorkers()
        {
            if (stageDatabase == null || stageManager == null)
            {
                Debug.LogWarning("StageDatabase or StageManager is missing.");
                return;
            }

            // 現在のステージデータを取得
            int stageIndex = stageManager.CurrentStageIndex;
            var stageData = stageDatabase.GetStageData(stageIndex);

            if (stageData == null) return;

            // 人数を取得 (StageDataに追加が必要)
            int workerCount = stageData.initialWorkerCount;

            // スケール調整
            AdjustScale(workerCount);

            // 生成
            GeneratedWorkers.Clear();

            // 破棄させる前にコルーチンを強制終了
            StopAllCoroutines();
            var taskProgresser = FindFirstObjectByType<Components.Game.Graph.Scripts.TaskProgresser>();
            if (taskProgresser != null)
            {
                taskProgresser.StopAllCoroutines();
            }

            // 以前のWorkerが残っているかもしれないので、子オブジェクトをすべて破壊
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < workerCount; i++)
            {
                // プレハブリストの範囲内で循環させる、あるいはランダムにするなどの仕様が必要ですが、
                // ここではリストの順番通り（足りなければループ）として実装します。
                if (workerPrefabs.Count == 0) break;

                GameObject prefab = workerPrefabs[i % workerPrefabs.Count];
                GameObject worker = Instantiate(prefab, transform);
                
                // 生成されたワーカーの名前を設定（検索しやすくするため）
                // WorkerA, WorkerB ... のように命名
                worker.name = "Worker" + (char)('A' + i);

                GeneratedWorkers.Add(worker);

                // 位置設定
                
                // ワーカーの高さが verticalSpacing に近いと仮定した場合の「真ん中上」合わせです。
                float topOffset = verticalSpacing * 0.5f;
                
                // リストの下に追加していくイメージで負方向
                float posY = -i * verticalSpacing - topOffset;
                worker.transform.localPosition = new Vector3(0f, posY, 0f);
            }

            // 生成したワーカーをTaskProgresserに渡す
            if (taskProgresser != null)
            {
                taskProgresser.SetWorkers(GeneratedWorkers);
            }
        }

        private void AdjustScale(int count)
        {
            if (count <= 0) return;

            float newScale = 1f;

            // リストのインデックスは count - 1 (1人ならindex 0)
            int index = count - 1;

            if (index >= 0 && index < scaleSettings.Count)
            {
                newScale = scaleSettings[index];
            }
            else
            {
                // 設定がない場合は、リストの最後の値を使うか、デフォルト1にする
                if (scaleSettings.Count > 0)
                {
                    newScale = scaleSettings[scaleSettings.Count - 1];
                }
            }
            
            transform.localScale = Vector3.one * newScale;
        }
    }
}
