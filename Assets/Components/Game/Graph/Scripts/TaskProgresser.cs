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
                        
                        // Calculate Height for Pivot Simulation
                        // If Pivot is Center (default), scaling Y shrinks from both top and bottom.
                        // To simulate Bottom Pivot (shrinking downwards to bottom), we need to move position DOWN as we shrink.
                        // Wait, if we want it to be anchored at Bottom Center, and shrink Y:
                        // Top should move down, Bottom should stay fixed.
                        // Center = Bottom + Height/2.
                        // New Center = Bottom + NewHeight/2.
                        // Delta Center = (NewHeight - Height) / 2.
                        // Since NewHeight < Height, Delta is negative (moves down).
                        
                        // NOTE: User request "黒マスクのbottom centerを基準としたい"
                        // Usually masks are covering the content.
                        // If it's a progress bar filling UP, the mask should shrink from TOP to BOTTOM?
                        // Or shrink from Bottom to Top?
                        // "逆方向に動かして" implies revealing.
                        // If content is static, Mask covers it.
                        // To reveal from bottom up: Mask Top moves up? No.
                        // Mask Bottom moves up? Revealing bottom.
                        // That means Mask is anchored at TOP?
                        // If mask is anchored at Bottom, scaling Y means top comes down. This reveals Top.
                        // Usually fill amounts go from Bottom to Top.
                        // So we want to REVEAL from Bottom to Top.
                        // This means the Mask should shrink upwards (Bottom moves up)?
                        // OR Mask moves/shrinks such that bottom part becomes visible first.
                        // If Mask is on top, and we shrink Y:
                        // If Pivot is Bottom: Top comes down. Bottom stays. -> Reveals Top (if background is white).
                        // If Pivot is Top: Bottom goes up. Top stays. -> Reveals Bottom.
                        
                        // User said "bottom centerを基準としたい".
                        // This literally means pivot is at Bottom Center.
                        // If we shrink scale Y with Bottom Pivot: The Top edge comes down. The Bottom edge stays fixed.
                        // This visual effect is: The bar shrinks downwards.
                        // This reveals whatever is *above* the mask? Or if the mask *is* the bar?
                        // "GaugeMask...黒いマスク...上下させることで進捗...逆方向に"
                        // If it's a mask covering the "filled" state.
                        // We want to reveal the fill from bottom to top (standard fill).
                        // So the Mask needs to disappear from bottom to top.
                        // This means Mask Bottom edge moves UP.
                        // This implies Mask Pivot should be TOP. (Shrinking Y pulls bottom up).
                        // BUT user explicitly asked "bottom centerを基準としたい".
                        // Maybe the Mask *IS* the fill? "GaugeMask...黒いマスク". Usually implies it blocks view.
                        // If it's a mask, and pivot is Bottom: It shrinks down.
                        // This means the top part becomes empty (revealed?).
                        // If the underlying sprite is the "Empty" state and Mask is "Fill"? No, "Mask" usually hides "Fill".
                        
                        // Let's assume user knows what they want: Pivot at Bottom Center.
                        // Behavior: Scale Y shrinks -> Top comes down.
                        // I will implement Pivot simulation for Bottom Center.
                        // Delta Pos = (NewScale.y - InitialScale.y) * Height / 2 (If localScale=1 is Height)
                        
                        RectTransform rt = mask.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            task.maskHeight = rt.rect.height;
                        }
                        else
                        {
                            SpriteRenderer sr = mask.GetComponent<SpriteRenderer>();
                            if (sr != null) task.maskHeight = sr.bounds.size.y; // Bounds size in world? Need local.
                            // For Sprite, local bounds size Y * localScale.y is effective height.
                            // Let's use Sprite.bounds.size.y / transform.lossyScale.y to get unscaled local size?
                            // Simplified: task.maskHeight = sr.sprite.bounds.size.y;
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
                if(t.spriteRenderer != null) { var c=t.spriteRenderer.color; c.a=1f; t.spriteRenderer.color=c; }
                if(t.uiImage != null) { var c=t.uiImage.color; c.a=1f; t.uiImage.color=c; }
            }
            UpdateVisualsImmediate(); 
        }

        private void ProcessWorker(Worker worker)
        {
            if (worker.isWorking)
            {
                if (worker.currentTask != null)
                {
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
                worker.currentTask.currentTime += timeReduction;

                if (worker.currentTask.currentTime >= worker.currentTask.totalTime)
                {
                    // Handled in Update
                }
                
                Debug.Log($"Reduced time for Worker {worker.label} by {timeReduction}. Current: {worker.currentTask.currentTime}/{worker.currentTask.totalTime}");
            }
            else
            {
                Debug.Log($"Worker {workerIndex} is not currently working on a task.");
            }
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
                        // Center moves DOWN by half the scale difference
                        // DeltaScaleY = currentScaleY - initialScaleY (negative)
                        // Shift Y = DeltaScaleY * Height / 2
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
            if (task.maskRenderer != null)
            {
                Color c = task.maskRenderer.color;
                c.a = alpha;
                task.maskRenderer.color = c;
            }
            else if (task.maskImage != null)
            {
                Color c = task.maskImage.color;
                c.a = alpha;
                task.maskImage.color = c;
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
