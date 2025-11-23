using UnityEngine;
using Components.Game.Graph.Scripts;

public class WorkerAnimationController : MonoBehaviour
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

    private Animator animator;
    private TaskProgresser taskProgresser;

    // 現在の状態を保持するための列挙型
    private enum WorkerState
    {
        None, // 初期状態
        Idle,
        Work,
        End
    }

    private WorkerState currentState = WorkerState.None;

    // トリガー名の定数
    private const string TriggerIdle = "Idle";
    private const string TriggerWork = "Work";
    private const string TriggerEnd = "End";

    void Start()
    {
        animator = GetComponent<Animator>();
        taskProgresser = FindFirstObjectByType<TaskProgresser>();
    }

    void Update()
    {
        if (taskProgresser == null || animator == null) return;

        // 次の状態を判定
        WorkerState nextState = DetermineNextState();

        // 状態変化に関わらず、その状態である限りトリガーを送り続ける
        SetAnimationTrigger(nextState);
        currentState = nextState;
    }

    private WorkerState DetermineNextState()
    {
        // 全タスク完了時は End
        if (taskProgresser.AreAllTasksCompleted())
        {
            return WorkerState.End;
        }

        // 作業中は Work
        int workerIndex = (int)workerType;
        if (taskProgresser.IsWorkerWorking(workerIndex))
        {
            return WorkerState.Work;
        }

        // それ以外は Idle
        return WorkerState.Idle;
    }

    private void SetAnimationTrigger(WorkerState state)
    {
        switch (state)
        {
            case WorkerState.Idle:
                animator.SetTrigger(TriggerIdle);
                break;
            case WorkerState.Work:
                animator.SetTrigger(TriggerWork);
                break;
            case WorkerState.End:
                animator.SetTrigger(TriggerEnd);
                break;
        }
    }
}
