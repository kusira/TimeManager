using System.Collections; // 追加
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

namespace Components.Game.Graph.Scripts
{
    public class TaskProgresser : MonoBehaviour
    {
        [System.Serializable]
        public class TaskNode
        {
            // ... (既存のフィールド)
            public int index;
            public float totalTime;
            public float currentTime;
            public List<int> dependencies = new List<int>(); 
            public bool isCompleted;
            public int assignedWorkerIndex; 
            
            public GameObject visualObject;
            public Vector3 vertexInitialScale; 
            
            public SpriteRenderer spriteRenderer; 
            public Image uiImage; 
            public CanvasGroup canvasGroup; 

            // Mask Animation
            public Transform gaugeMask;
            public SpriteRenderer maskRenderer; 
            public Image maskImage; 
            
            public Vector3 maskInitialScale;
            public Vector3 maskInitialPos; 
            public float maskHeight; 

            // Edges
            public List<GameObject> outgoingEdges = new List<GameObject>();
            public List<Vector3> outgoingEdgesInitialScale = new List<Vector3>();
            public List<Vector3> outgoingEdgesInitialPos = new List<Vector3>();
            public List<Vector3> outgoingEdgesTargetPos = new List<Vector3>();

            // State tracking
            public bool isWorkable; 
            public float currentMaskAlpha = 0.9f; 
            
            public float targetMaskAlpha = 0.9f;
            
            // Completion Animation
            public bool isAnimatingCompletion = false;
            public float completionAnimTime = 0f; 

            // Bonus Animation
            public float remainingTimeReduction = 0f; 
            public float reductionSpeed = 0f; 
            public bool isApplyingBonus = false;
            public Color defaultMaskColor; 
            public bool hasSavedColor = false;
        }

        [System.Serializable]
        public class Worker
        {
            public string label; 
            public int workerIndex; 
            public GameObject workerObject; 
            public TMP_Text remainText; 
            public TaskNode currentTask;
            public bool isWorking;
            public float nextTaskTimer; // 次のタスク開始までのタイマー
            public bool isUiShowing; // 追加: UIの表示目標状態
            public Coroutine fadeCoroutine;
            public Vector3 uiOriginalLocalPos;
            public Vector3 uiInitialScale;
        }

        [SerializeField] private GraphGenerator GraphGenerator;
        
        [System.Serializable]
        public class WorkerGameObject
        {
            public string name;
            public GameObject gameObject;
        }

        [SerializeField] private List<WorkerGameObject> workerObjects = new List<WorkerGameObject>
        {
            new WorkerGameObject { name = "Worker A" },
            new WorkerGameObject { name = "Worker B" },
            new WorkerGameObject { name = "Worker C" },
            new WorkerGameObject { name = "Worker D" },
            new WorkerGameObject { name = "Worker E" }
        };

        [Header("UI Animation Settings")]
        [SerializeField] private float textFadeDuration = 0.1f;
        [SerializeField] private float textWaitDuration = 0.1f; // 追加
        [SerializeField] private float uiAnimationOffset = 0.3f;

        [Header("Bonus Animation Settings")]
        [SerializeField] private float bonusAnimDuration = 0.15f;
        [SerializeField] private Color bonusMaskColor = new Color(1f, 1f, 0.5f); // 少し黄色
        [SerializeField] private float nextTaskInterval = 0.3f; // タスク間のインターバル

        [Header("Debug Info (デバッグ用)")]
        [SerializeField] private List<TaskNode> allTasks = new List<TaskNode>();
        [SerializeField] private List<Worker> workers = new List<Worker>();

        [SerializeField] private Components.Game.Canvas.Scripts.ResultManager resultManager;
        [SerializeField] private Components.Game.Canvas.Scripts.TimeLimitManager timeLimitManager; // 追加

        private bool isInitialized = false;
        private const float AlphaTransitionSpeed = 1.0f / 0.3f; 
        private const float CompletionAnimDuration = 0.3f;

        private void Awake()
        {
            AssignWorkerObjectsAutomatically();
        }

        private void Start()
        {
            if (resultManager == null) resultManager = FindFirstObjectByType<Components.Game.Canvas.Scripts.ResultManager>();
            if (timeLimitManager == null) timeLimitManager = FindFirstObjectByType<Components.Game.Canvas.Scripts.TimeLimitManager>(); // 追加
            // Auto initialize if possible
            // Initialize(); 
        }

        private void AssignWorkerObjectsAutomatically()
        {
            // Workerリストが空、または要素数が足りない場合のためのフォールバック
            // Inspectorで設定されている想定だが、動的確保も考慮
            if (workerObjects == null) workerObjects = new List<WorkerGameObject>();
            
            // 5人分（A-E）確保
            for (int i = workerObjects.Count; i < 5; i++)
            {
                workerObjects.Add(new WorkerGameObject { name = "Worker " + (char)('A' + i) });
            }

            for (int i = 0; i < workerObjects.Count; i++)
            {
                // 既にアサインされていればスキップ
                if (workerObjects[i].gameObject != null) continue;

                char workerChar = (char)('A' + i);
                // "WorkerA", "WorkerB" ... のパターンを検索
                string targetName1 = "Worker" + workerChar; 
                // "Worker A", "Worker B" ... のパターンも一応検索
                string targetName2 = "Worker " + workerChar;

                GameObject found = GameObject.Find(targetName1);
                if (found == null) found = GameObject.Find(targetName2);

                if (found != null)
                {
                    workerObjects[i].gameObject = found;
                }
            }
        }

        private void Update()
        {
            if (!isInitialized)
            {
                Initialize();
                if (!isInitialized) return;
            }

            foreach (var worker in workers)
            {
                ProcessWorker(worker);
                UpdateWorkerUI(worker); // UI更新を追加
            }

            UpdateVisualization();
        }

        private void UpdateWorkerUI(Worker worker)
        {
            if (worker.remainText == null) return;

            bool shouldShow = worker.isWorking && worker.currentTask != null;

            // 目標状態が変わったらアニメーション開始
            if (shouldShow != worker.isUiShowing)
            {
                worker.isUiShowing = shouldShow;
                if (worker.fadeCoroutine != null) StopCoroutine(worker.fadeCoroutine);

                if (shouldShow)
                {
                    worker.fadeCoroutine = StartCoroutine(ShowWorkerUISequence(worker));
                }
                else
                {
                    worker.fadeCoroutine = StartCoroutine(HideWorkerUISequence(worker));
                }
            }
            
            // テキスト更新（表示中の場合）
            if (shouldShow)
            {
                float remaining = Mathf.Max(0f, worker.currentTask.totalTime - worker.currentTask.currentTime);
                worker.remainText.text = remaining.ToString("F1");
            }
            else if (worker.remainText.gameObject.activeSelf && worker.isWorking == false) // フェードアウト中かつ次のタスク未開始
            {
                worker.remainText.text = "0.0";
            }
        }

        // 表示シーケンス: (FadeOut -> Wait) -> FadeIn
        private IEnumerator ShowWorkerUISequence(Worker worker)
        {
            if (worker.remainText == null) yield break;

            // CanvasGroupは使用しない
            float currentAlpha = worker.remainText.color.a;

            // 既に表示中あるいはフェードアウト中の場合、一度完全にフェードアウトさせる
            // Alpha > 0.01f なら "残像" があるとみなして、消去アニメーション + 待機 を入れる
            if (worker.remainText.gameObject.activeSelf && currentAlpha > 0.01f)
            {
                // 下へフェードアウト
                yield return AnimateAlphaPos(worker, currentAlpha, 0f, worker.remainText.transform.localPosition, worker.uiOriginalLocalPos - Vector3.up * uiAnimationOffset, textFadeDuration);
                
                // 待機
                yield return new WaitForSeconds(textWaitDuration);
            }

            // 表示開始
            worker.remainText.gameObject.SetActive(true);
            
            // 下から上へフェードイン
            Vector3 startPos = worker.uiOriginalLocalPos - Vector3.up * uiAnimationOffset;
            Vector3 endPos = worker.uiOriginalLocalPos;
            
            // 初期位置セット
            worker.remainText.transform.localPosition = startPos;
            worker.remainText.transform.localScale = worker.uiInitialScale;
            
            // Alpha初期化
            SetComponentsAlpha(worker, 0f);

            yield return AnimateAlphaPos(worker, 0f, 1f, startPos, endPos, textFadeDuration);
            
            worker.fadeCoroutine = null;
        }

        // 非表示シーケンス: FadeOut Only
        private IEnumerator HideWorkerUISequence(Worker worker)
        {
            if (worker.remainText == null) yield break;
            if (!worker.remainText.gameObject.activeSelf) yield break;

            float currentAlpha = worker.remainText.color.a;

            // 下へフェードアウト
            Vector3 startPos = worker.remainText.transform.localPosition;
            Vector3 endPos = worker.uiOriginalLocalPos - Vector3.up * uiAnimationOffset;
            
            yield return AnimateAlphaPos(worker, currentAlpha, 0f, startPos, endPos, textFadeDuration);
            
            worker.remainText.gameObject.SetActive(false);
            worker.fadeCoroutine = null;
        }

        // 汎用アニメーションコルーチン
        private IEnumerator AnimateAlphaPos(Worker worker, float startAlpha, float endAlpha, Vector3 startPos, Vector3 endPos, float duration)
        {
             Transform target = worker.remainText.transform;
             
             // コンポーネント取得
             List<TMP_Text> texts = new List<TMP_Text>();
             List<SpriteRenderer> sprites = new List<SpriteRenderer>();
             List<Image> images = new List<Image>();
             
             worker.remainText.GetComponentsInChildren<TMP_Text>(true, texts);
             worker.remainText.GetComponentsInChildren<SpriteRenderer>(true, sprites);
             worker.remainText.GetComponentsInChildren<Image>(true, images);

             float timer = 0f;
             while (timer < duration)
             {
                 timer += Time.deltaTime;
                 float t = Mathf.Clamp01(timer / duration);
                 float easeT = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f; // EaseInOutSine

                 float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, easeT);
                 
                 // Apply Alpha
                 foreach(var txt in texts) { Color c = txt.color; c.a = currentAlpha; txt.color = c; }
                 foreach(var spr in sprites) { Color c = spr.color; c.a = currentAlpha; spr.color = c; }
                 foreach(var img in images) { Color c = img.color; c.a = currentAlpha; img.color = c; }

                 target.localPosition = Vector3.Lerp(startPos, endPos, easeT);
                 target.localScale = worker.uiInitialScale;
                 yield return null;
             }
             
             // Ensure end alpha
             foreach(var txt in texts) { Color c = txt.color; c.a = endAlpha; txt.color = c; }
             foreach(var spr in sprites) { Color c = spr.color; c.a = endAlpha; spr.color = c; }
             foreach(var img in images) { Color c = img.color; c.a = endAlpha; img.color = c; }

             target.localPosition = endPos;
             target.localScale = worker.uiInitialScale;
        }

        private void SetComponentsAlpha(Worker worker, float alpha)
        {
             if (worker.remainText == null) return;
             var texts = worker.remainText.GetComponentsInChildren<TMP_Text>(true);
             var sprites = worker.remainText.GetComponentsInChildren<SpriteRenderer>(true);
             var images = worker.remainText.GetComponentsInChildren<Image>(true);
             
             foreach(var txt in texts) { Color c = txt.color; c.a = alpha; txt.color = c; }
             foreach(var spr in sprites) { Color c = spr.color; c.a = alpha; spr.color = c; }
             foreach(var img in images) { Color c = img.color; c.a = alpha; img.color = c; }
        }

        public void Initialize()
        {
            if (GraphGenerator == null)
            {
                GraphGenerator = FindFirstObjectByType<GraphGenerator>();
            }

            if (GraphGenerator == null) return;

            var vertices = GraphGenerator.GetVertices();
            var edges = GraphGenerator.GetEdges();
            var vertexObjects = GraphGenerator.GetGeneratedVertexObjects();

            if (vertices == null || vertices.Count == 0 || vertexObjects == null || vertexObjects.Count == 0)
            {
                return;
            }

            allTasks.Clear();
            workers.Clear();

            HashSet<int> requiredWorkerIndices = new HashSet<int>();

            // Build Tasks
            for (int i = 0; i < vertices.Count; i++)
            {
                var vData = vertices[i];
                int workerIdx = vData.i; 
                requiredWorkerIndices.Add(workerIdx);

                var task = new TaskNode
                {
                    index = i,
                    totalTime = vData.taskCompletionTime,
                    currentTime = 0f,
                    isCompleted = false,
                    assignedWorkerIndex = workerIdx,
                    visualObject = vertexObjects[i],
                    vertexInitialScale = vertexObjects[i].transform.localScale, 
                    currentMaskAlpha = 0.9f, 
                    targetMaskAlpha = 0.9f,
                    isWorkable = false
                };

                // Cache Components
                task.spriteRenderer = task.visualObject.GetComponent<SpriteRenderer>();
                if (task.spriteRenderer == null) task.spriteRenderer = task.visualObject.GetComponentInChildren<SpriteRenderer>();
                
                task.uiImage = task.visualObject.GetComponent<Image>();
                if (task.uiImage == null) task.uiImage = task.visualObject.GetComponentInChildren<Image>();

                task.canvasGroup = task.visualObject.GetComponent<CanvasGroup>();

                // Find GaugeMask
                Transform circle = task.visualObject.transform.Find("Circle");
                if (circle != null)
                {
                    Transform mask = circle.Find("GaugeMask");
                    if (mask != null)
                    {
                        task.gaugeMask = mask;
                        task.maskInitialScale = mask.localScale;
                        task.maskInitialPos = mask.localPosition;
                        
                        task.maskRenderer = mask.GetComponent<SpriteRenderer>();
                        task.maskImage = mask.GetComponent<Image>();
                        
                        // Save default color
                        if (task.maskRenderer != null)
                        {
                            task.defaultMaskColor = task.maskRenderer.color;
                            task.hasSavedColor = true;
                        }
                        else if (task.maskImage != null)
                        {
                            task.defaultMaskColor = task.maskImage.color;
                            task.hasSavedColor = true;
                        }

                        // Calculate Height for Pivot Simulation
                        RectTransform rt = mask.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            task.maskHeight = rt.rect.height;
                        }
                        else
                        {
                            SpriteRenderer sr = mask.GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null)
                                task.maskHeight = sr.sprite.bounds.size.y;
                            else
                                task.maskHeight = 1.0f;
                        }
                    }
                }

                allTasks.Add(task);
            }

            // Create Workers
            foreach(int idx in requiredWorkerIndices)
            {
                string label = ((char)('A' + idx)).ToString();
                GameObject wObj = null;
                TMP_Text rText = null;
                Vector3 originalPos = Vector3.zero;

                if (idx < workerObjects.Count)
                {
                    wObj = workerObjects[idx].gameObject;
                    if (wObj != null)
                    {
                        Transform uiTrans = wObj.transform.Find("UI");
                        if (uiTrans != null)
                        {
                            Transform textTrans = uiTrans.Find("RemainText");
                            if (textTrans != null)
                            {
                                rText = textTrans.GetComponent<TMP_Text>();
                            }
                        }
                        // フォールバック検索
                        if (rText == null)
                        {
                            var texts = wObj.GetComponentsInChildren<TMP_Text>(true);
                            foreach (var t in texts)
                            {
                                if (t.name == "RemainText")
                                {
                                    rText = t;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (rText != null)
                {
                    originalPos = rText.transform.localPosition;
                }

                workers.Add(new Worker
                {
                    label = label,
                    workerIndex = idx,
                    workerObject = wObj,
                    remainText = rText,
                    isWorking = false,
                    currentTask = null,
                    uiOriginalLocalPos = originalPos,
                    uiInitialScale = rText != null ? rText.transform.localScale : Vector3.one, // スケール保存
                    nextTaskTimer = 0f // 初期化
                });
            }
            workers.Sort((a, b) => a.workerIndex.CompareTo(b.workerIndex));

            // Build Dependencies and Find Edges
            Transform graphParent = vertexObjects[0].transform.parent;

            foreach (var edge in edges)
            {
                if (edge.toIndex < allTasks.Count && edge.fromIndex < allTasks.Count)
                {
                    allTasks[edge.toIndex].dependencies.Add(edge.fromIndex);

                    string edgeName = $"Edge_{edge.fromIndex}_{edge.toIndex}";
                    Transform edgeTrans = graphParent.Find(edgeName);
                    if (edgeTrans != null)
                    {
                        var outgoingTask = allTasks[edge.fromIndex];
                        outgoingTask.outgoingEdges.Add(edgeTrans.gameObject);
                        
                        outgoingTask.outgoingEdgesInitialScale.Add(edgeTrans.localScale);
                        outgoingTask.outgoingEdgesInitialPos.Add(edgeTrans.localPosition);
                        
                        Transform endVertexT = vertexObjects[edge.toIndex].transform;
                        outgoingTask.outgoingEdgesTargetPos.Add(endVertexT.localPosition);
                    }
                }
            }

            isInitialized = true;
            CheckWorkableStatus();
            // Snap to initial state
            foreach(var t in allTasks) 
            {
                t.currentMaskAlpha = t.targetMaskAlpha;
                ApplyMaskAlpha(t, t.currentMaskAlpha); // 初期化時に色も戻す
            }
            UpdateVisualsImmediate(); 
        }

        private void ProcessWorker(Worker worker)
        {
            if (worker.isWorking)
            {
                if (worker.currentTask != null)
                {
                    // ボーナス時間の加算処理
                    if (worker.currentTask.remainingTimeReduction > 0)
                    {
                        worker.currentTask.isApplyingBonus = true;
                        float amount = worker.currentTask.reductionSpeed * Time.deltaTime;
                        
                        // 残りを超えないように
                        if (amount > worker.currentTask.remainingTimeReduction)
                        {
                            amount = worker.currentTask.remainingTimeReduction;
                        }
                        
                        worker.currentTask.currentTime += amount;
                        worker.currentTask.remainingTimeReduction -= amount;
                    }
                    else
                    {
                        worker.currentTask.isApplyingBonus = false;
                    }

                    // 通常時間の加算
                    worker.currentTask.currentTime += Time.deltaTime;
                    
                    if (worker.currentTask.currentTime >= worker.currentTask.totalTime)
                    {
                        CompleteTask(worker);
                    }
                }
                else
                {
                    worker.isWorking = false; 
                }
            }
            else
            {
                // インターバル処理
                if (worker.nextTaskTimer > 0f)
                {
                    worker.nextTaskTimer -= Time.deltaTime;
                    if (worker.nextTaskTimer > 0f) return; // まだインターバル中なら何もしない
                }

                TaskNode nextTask = null;

                for (int i = 0; i < allTasks.Count; i++)
                {
                    var task = allTasks[i];
                    
                    if (task.assignedWorkerIndex == worker.workerIndex 
                        && !task.isCompleted 
                        && task.currentTime == 0) 
                    {
                        if (task.isWorkable)
                        {
                            nextTask = task;
                            break; 
                        }
                    }
                }

                if (nextTask != null)
                {
                    worker.currentTask = nextTask;
                    worker.isWorking = true;
                }
            }
        }

        private void CompleteTask(Worker worker)
        {
            if (worker.currentTask != null)
            {
                worker.currentTask.isCompleted = true;
                worker.currentTask.currentTime = worker.currentTask.totalTime; 
                
                // ボーナス処理も終了
                worker.currentTask.remainingTimeReduction = 0f;
                worker.currentTask.isApplyingBonus = false;

                worker.currentTask.isAnimatingCompletion = true;
                worker.currentTask.completionAnimTime = 0f;

                worker.currentTask = null;
                worker.isWorking = false;
                worker.nextTaskTimer = nextTaskInterval; // インターバル設定
                
                CheckWorkableStatus(); 

                if (AreAllTasksCompleted())
                {
                    // タイマー停止
                    if (timeLimitManager != null)
                    {
                        timeLimitManager.StopTimer();
                    }

                    if (resultManager != null)
                    {
                        resultManager.ShowGameClear();
                    }
                }
            }
        }

        public void ReduceTaskTime(int workerIndex, float timeReduction)
        {
            var worker = workers.Find(w => w.workerIndex == workerIndex);
            if (worker != null && worker.isWorking && worker.currentTask != null)
            {
                // 即時加算ではなく、残り時間に積む
                worker.currentTask.remainingTimeReduction += timeReduction;
                
                // 速度を計算（0.15秒で消化、既に加算中なら再計算）
                // 単純に固定時間で消化する場合、残量が増えると速度が上がる
                worker.currentTask.reductionSpeed = worker.currentTask.remainingTimeReduction / bonusAnimDuration;
                
                Debug.Log($"Bonus added for Worker {worker.label}. Amount: {timeReduction}, New Remaining: {worker.currentTask.remainingTimeReduction}");
            }
            else
            {
                Debug.Log($"Worker {workerIndex} is not currently working on a task.");
            }
        }

        public bool IsWorkerWorking(int workerIndex)
        {
            var worker = workers.Find(w => w.workerIndex == workerIndex);
            return worker != null && worker.isWorking;
        }

        public bool AreAllTasksCompleted()
        {
            if (allTasks == null || allTasks.Count == 0) return false;
            return allTasks.All(t => t.isCompleted);
        }

        // TaskQueueMover用に、指定したWorkerのタスク一覧（未完了のみ）を取得する
        public List<TaskNode> GetWorkerTasks(int workerIndex)
        {
            List<TaskNode> tasks = new List<TaskNode>();
            foreach (var task in allTasks)
            {
                if (task.assignedWorkerIndex == workerIndex && !task.isCompleted)
                {
                    tasks.Add(task);
                }
            }
            // タスクIndex順にソート（必要なら）
            tasks.Sort((a, b) => a.index.CompareTo(b.index));
            return tasks;
        }

        // 現在進行中のタスクの残り時間を取得する
        public float GetWorkerCurrentTaskRemainingTime(int workerIndex)
        {
            var worker = workers.Find(w => w.workerIndex == workerIndex);
            if (worker != null && worker.isWorking && worker.currentTask != null)
            {
                return Mathf.Max(0f, worker.currentTask.totalTime - worker.currentTask.currentTime);
            }
            return 0f;
        }

        public GameObject GetWorkerGameObject(int workerIndex)
        {
            if (workerObjects != null && workerIndex >= 0 && workerIndex < workerObjects.Count)
            {
                return workerObjects[workerIndex].gameObject;
            }
            return null;
        }

        private void CheckWorkableStatus()
        {
            foreach (var task in allTasks)
            {
                bool dependenciesMet = true;
                foreach (var depIndex in task.dependencies)
                {
                    if (!allTasks[depIndex].isCompleted)
                    {
                        dependenciesMet = false;
                        break;
                    }
                }

                task.isWorkable = dependenciesMet;

                if (task.isWorkable || task.isCompleted)
                {
                    task.targetMaskAlpha = 0.7f; 
                }
                else
                {
                    task.targetMaskAlpha = 0.9f; 
                }
            }
        }

        private void UpdateVisualization()
        {
            foreach (var task in allTasks)
            {
                if (task.isAnimatingCompletion)
                {
                    task.completionAnimTime += Time.deltaTime;
                    float t = Mathf.Clamp01(task.completionAnimTime / CompletionAnimDuration);
                    
                    Vector3 currentScale = Vector3.Lerp(task.vertexInitialScale, Vector3.zero, t);
                    if (task.visualObject != null)
                    {
                        task.visualObject.transform.localScale = currentScale;
                    }

                    for (int i = 0; i < task.outgoingEdges.Count; i++)
                    {
                        var edgeObj = task.outgoingEdges[i];
                        if(edgeObj != null)
                        {
                            Vector3 initialScale = task.outgoingEdgesInitialScale[i];
                            Vector3 initialPos = task.outgoingEdgesInitialPos[i];
                            Vector3 targetPos = task.outgoingEdgesTargetPos[i];

                            Vector3 currentEdgeScale = initialScale;
                            currentEdgeScale.x = Mathf.Lerp(initialScale.x, 0f, t);
                            edgeObj.transform.localScale = currentEdgeScale;

                            edgeObj.transform.localPosition = Vector3.Lerp(initialPos, targetPos, t);
                        }
                    }
                }

                if (!task.isAnimatingCompletion)
                {
                    if (!Mathf.Approximately(task.currentMaskAlpha, task.targetMaskAlpha))
                    {
                        task.currentMaskAlpha = Mathf.MoveTowards(task.currentMaskAlpha, task.targetMaskAlpha, AlphaTransitionSpeed * Time.deltaTime);
                    }
                    
                    ApplyMaskAlpha(task, task.currentMaskAlpha);

                    if (task.gaugeMask != null)
                    {
                        float progress = 0f;
                        if (task.totalTime > 0) progress = Mathf.Clamp01(task.currentTime / task.totalTime);
                        else progress = task.isCompleted ? 1f : 0f;

                        // Scale Logic with Simulated Bottom-Center Pivot
                        // Target Scale Y goes from Initial to 0
                        float currentScaleY = Mathf.Lerp(task.maskInitialScale.y, 0f, progress);
                        Vector3 newScale = task.maskInitialScale;
                        newScale.y = currentScaleY;
                        task.gaugeMask.localScale = newScale;

                        // Position adjustment for Bottom Pivot
                        float deltaY = (currentScaleY - task.maskInitialScale.y) * task.maskHeight * 0.5f;
                        
                        Vector3 newPos = task.maskInitialPos;
                        newPos.y += deltaY;
                        task.gaugeMask.localPosition = newPos;
                    }
                }
            }
        }

        private void ApplyMaskAlpha(TaskNode task, float alpha)
        {
            Color targetColor = task.hasSavedColor ? task.defaultMaskColor : Color.black;
            
            // ボーナス適用中なら黄色くする
            if (task.isApplyingBonus)
            {
                targetColor = bonusMaskColor;
            }

            targetColor.a = alpha;

            if (task.maskRenderer != null)
            {
                task.maskRenderer.color = targetColor;
            }
            else if (task.maskImage != null)
            {
                task.maskImage.color = targetColor;
            }
        }

        private void UpdateVisualsImmediate()
        {
             foreach (var task in allTasks)
            {
                ApplyMaskAlpha(task, task.currentMaskAlpha);
            }
        }
    }
}
