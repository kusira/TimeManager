using UnityEngine;
using System.Collections;

public class OpenAndClose : MonoBehaviour
{
    [SerializeField] private Transform moveTarget;  // ← 動かす対象（新規）
    [SerializeField] private Transform startPos;    // 待機位置（閉じている位置）
    [SerializeField] private Transform endPos;      // 表示位置（開いた位置）
    [SerializeField] private float speed = 8f;      // 移動速度

    private Coroutine moveRoutine;
    private bool isOpen = false; 

    void Start()
    {
        // 初期位置を startPos に
        if (moveTarget != null && startPos != null)
            moveTarget.position = startPos.position;
    }

    public void Open()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveTo(endPos.position));
    }

    public void Close()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveTo(startPos.position));
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(moveTarget.position, target) > 0.01f)
        {
            moveTarget.position = Vector3.MoveTowards(
                moveTarget.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }

        moveTarget.position = target;
        moveRoutine = null;
    }
}
