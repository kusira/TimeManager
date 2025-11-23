using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Components.Game.Graph.Scripts;

namespace Components.Game.Workers.Scripts
{
    public class TaskQueueMover : MonoBehaviour
    {
        public enum WorkerType
        {
            A,
            B,
            C,
            D,
            E
        }

        [Tooltip("Workerのタイプを選択 (A=0, B=1, ...)")]
        [SerializeField] private WorkerType workerType;

        [Header("Queue Settings")]
        [SerializeField] private GameObject taskPrefab;
        [SerializeField] private Transform taskParent;
        [SerializeField] private float spacingX = 1.5f;
        [SerializeField] private int maxTasksDisplayed = 3;
        [SerializeField] private float moveDuration = 0.3f;

        // Workerへの参照はTaskProgresser側で管理・更新するため削除
        // [Header("Worker References")]
        // [Tooltip("WorkerのGameObjectリスト (直下にUI > RemainTextがある想定)")]
        // [SerializeField] private List<GameObject> workerObjects;
        // private TMP_Text remainText;

        private TaskProgresser taskProgresser;
        private List<TaskProgresser.TaskNode> currentTasks = new List<TaskProgresser.TaskNode>();
        
        // 表示中のタスクオブジェクト (Key: Task Index, Value: GameObject)
        private Dictionary<int, GameObject> taskObjects = new Dictionary<int, GameObject>();
        private bool isInitialized = false;

        void Start()
        {
            taskProgresser = FindFirstObjectByType<TaskProgresser>();
        }

        void Update()
        {
            if (taskProgresser == null)
            {
                taskProgresser = FindFirstObjectByType<TaskProgresser>();
                if (taskProgresser == null) return;
            }

            UpdateQueue();
        }

        private void UpdateQueue()
        {
            if (taskProgresser == null) return;

            int workerIndex = (int)workerType;
            // 未完了タスクを取得
            var newTasks = taskProgresser.GetWorkerTasks(workerIndex);

            // 変更検知（先頭が変わった、数が減ったなど）
            if (!isInitialized || HasQueueChanged(newTasks))
            {
                RefreshQueueVisuals(newTasks);
                currentTasks = newTasks;
                isInitialized = true;
            }
        }

        private bool HasQueueChanged(List<TaskProgresser.TaskNode> newTasks)
        {
            if (currentTasks.Count != newTasks.Count) return true;
            
            for(int i=0; i<newTasks.Count; i++)
            {
                if (currentTasks[i].index != newTasks[i].index) return true;
            }
            return false;
        }

        private void RefreshQueueVisuals(List<TaskProgresser.TaskNode> newTasks)
        {
            // 表示すべきタスクのインデックスセット
            HashSet<int> displayedIndices = new HashSet<int>();
            int count = 0;

            // 先頭から最大N個を表示
            foreach (var task in newTasks)
            {
                if (count >= maxTasksDisplayed) break;
                displayedIndices.Add(task.index);
                count++;
            }

            // 不要になったオブジェクトを削除（完了したタスクなど）
            List<int> toRemove = new List<int>();
            foreach (var kvp in taskObjects)
            {
                if (!displayedIndices.Contains(kvp.Key))
                {
                    Destroy(kvp.Value);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (int idx in toRemove) taskObjects.Remove(idx);

            // 新しいタスクの生成と位置更新
            count = 0;
            foreach (var task in newTasks)
            {
                if (count >= maxTasksDisplayed) break;

                GameObject obj;
                if (!taskObjects.TryGetValue(task.index, out obj))
                {
                    // 新規生成
                    obj = Instantiate(taskPrefab, taskParent);
                    obj.name = $"Task_{task.index}";
                    
                    // TaskIndexを設定 (index+1表示)
                    Transform textTrans = obj.transform.Find("TaskIndex");
                    if (textTrans != null)
                    {
                        var tmp = textTrans.GetComponent<TMP_Text>();
                        if (tmp != null) tmp.text = (task.index + 1).ToString();
                    }

                    // 初期位置
                    obj.transform.localPosition = new Vector3(count * spacingX, 0, 0);
                    
                    taskObjects.Add(task.index, obj);
                }

                // 目標位置への移動
                Vector3 targetPos = new Vector3(count * spacingX, 0, 0);
                StartCoroutine(MoveToPosition(obj.transform, targetPos));

                count++;
            }
        }

        private IEnumerator MoveToPosition(Transform t, Vector3 targetPos)
        {
            if (t == null) yield break;
            
            Vector3 startPos = t.localPosition;
            float timer = 0f;

            // 既に近い場合は移動しない
            if (Vector3.Distance(startPos, targetPos) < 0.01f)
            {
                t.localPosition = targetPos;
                yield break;
            }

            while (timer < moveDuration)
            {
                if (t == null) yield break;
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / moveDuration);
                // EaseOut
                progress = Mathf.Sin(progress * Mathf.PI * 0.5f);
                
                t.localPosition = Vector3.Lerp(startPos, targetPos, progress);
                yield return null;
            }
            
            if (t != null) t.localPosition = targetPos;
        }
    }
}
