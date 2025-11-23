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
            public Vector3 maskInitialPos; // To adjust position during scaling if pivot is center
            public float maskHeight; // Original height to calculate offset

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
            public float remainingTimeReduction = 0f; // アニメーションで適用待ちの時間
            public float reductionSpeed = 0f; // 1秒あたりに適用する時間
            public bool isApplyingBonus = false;
            public Color defaultMaskColor; // 色変更からの復帰用
            public bool hasSavedColor = false;
        }

        [System.Serializable]
        public class Worker
        {
            public string label; 
            public int workerIndex; 
            public TaskNode currentTask;
            public bool isWorking;
        }

        [SerializeField] private GraphGenerator GraphGenerator;
        
        [SerializeField] private List<TaskNode> allTasks = new List<TaskNode>();
        [SerializeField] private List<Worker> workers = new List<Worker>();

        [Header("Bonus Animation Settings")]
        [SerializeField] private float bonusAnimDuration = 0.15f;
        [SerializeField] private Color bonusMaskColor = new Color(1f, 1f, 0.5f); // 少し黄色

        private bool isInitialized = false;
        private const float AlphaTransitionSpeed = 1.0f / 0.3f; 
        private const float CompletionAnimDuration = 0.3f;

        private void Start()
        {
            // Auto initialize if possible
            // Initialize(); 
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
            }

            UpdateVisualization();
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
                workers.Add(new Worker
                {
                    label = label,
                    workerIndex = idx,
                    isWorking = false,
                    currentTask = null
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
                
                CheckWorkableStatus(); 
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
