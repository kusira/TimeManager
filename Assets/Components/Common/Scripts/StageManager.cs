using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 追加
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

        private const string PREFS_KEY_MAX_STAGE = "MaxReachedStage";

        /// <summary>
        /// 到達した最大ステージを取得
        /// </summary>
        public static int GetMaxReachedStage()
        {
            return PlayerPrefs.GetInt(PREFS_KEY_MAX_STAGE, 0);
        }

        /// <summary>
        /// 最大到達ステージを更新して保存
        /// </summary>
        public static void UpdateMaxReachedStage(int stageIndex)
        {
            int currentMax = GetMaxReachedStage();
            if (stageIndex > currentMax)
            {
                PlayerPrefs.SetInt(PREFS_KEY_MAX_STAGE, stageIndex);
                PlayerPrefs.Save();
            }
        }

        // 次のシーンロード時に適用するステージインデックス
        private static int? PendingStageIndex = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // 新しいシーンで生成されたStageManagerから、シーン内の参照（UIなど）をシングルトンに引き継ぐ
                if (this.stageText != null)
                {
                    Instance.stageText = this.stageText;
                }
                
                // 他のInspector設定された参照も引き継ぐ
                if (this.graphGenerator != null) Instance.graphGenerator = this.graphGenerator;
                if (this.itemGenerator != null) Instance.itemGenerator = this.itemGenerator;
                if (this.timeLimitManager != null) Instance.timeLimitManager = this.timeLimitManager;

                // 参照更新後に初期化処理（ステージ進行含む）を再実行させる
                Instance.InitializeDependencies();

                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // シーンロードイベントの登録
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            InitializeDependencies();
        }

        private void OnDestroy()
        {
            // シングルトンインスタンスが破棄されるときのみ解除
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // シーン遷移時にも依存関係を再取得・初期化
            InitializeDependencies();
        }

        public void InitializeDependencies()
        {
            // PendingStageIndexがあれば適用
            if (PendingStageIndex.HasValue)
            {
                currentStageIndex = PendingStageIndex.Value;
                PendingStageIndex = null; // リセット
            }

            // 自動で探す (参照が切れている場合やnullの場合)
            if (graphGenerator == null) graphGenerator = FindFirstObjectByType<GraphGenerator>();
            if (itemGenerator == null) itemGenerator = FindFirstObjectByType<ItemGenerator>();
            if (timeLimitManager == null) timeLimitManager = FindFirstObjectByType<TimeLimitManager>();

            // UIテキストも再取得を試みる（シーン遷移で参照が切れるため）
            if (stageText == null)
            {
                // StageTextという名前のオブジェクトを探す、あるいはタグで探すなど
                // ここではとりあえず FindFirstObjectByType で TMP_Text を探すのは危険（他のが取れるかも）なので
                // 必要であれば "StageNumText" などの名前で探す等の処理を入れると良いですが、
                // 現状はシリアライズフィールド運用のようなので、nullチェックしつつ更新します。
            }

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
                if(currentStageIndex == 6)
                {
                    stageText.text = $"最終日";
                }
                else
                {
                    stageText.text = $"{currentStageIndex+1} 日目";
                }
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
            UpdateMaxReachedStage(PendingStageIndex.Value);
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

