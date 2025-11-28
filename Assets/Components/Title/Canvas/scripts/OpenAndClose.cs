using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class OpenAndClose : MonoBehaviour
{
    [SerializeField] private Transform moveTarget;  // ← 動かす対象（新規）
    [SerializeField] private Transform startPos;    // 待機位置（閉じている位置）
    [SerializeField] private Transform endPos;      // 表示位置（開いた位置）
    [SerializeField] private float speed = 8f;      // 移動速度
    [SerializeField] private RectTransform contentRect; // クリック判定を行う対象（この範囲外をクリックで閉じる）
    [SerializeField] private AudioSource clickAudioSource;

    private Coroutine moveRoutine;
    private bool isOpen = false;
    private Camera uiCamera;

    void Start()
    {
        // 初期位置を startPos に
        if (moveTarget != null && startPos != null)
            moveTarget.position = startPos.position;

        // CanvasのRenderModeに応じてカメラを取得
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }
    }

    void Update()
    {
        // 開いている時にクリック（またはタップ）されたら判定
        // 新しいInput System対応: Pointer.currentを使用することでマウスとタッチ両方に対応
        if (isOpen && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            // contentRectが設定されており、かつその範囲外がクリックされた場合
            // 第3引数にカメラを渡すことで、Screen Space - Camera や World Space のCanvasにも対応
            if (contentRect != null && !RectTransformUtility.RectangleContainsScreenPoint(contentRect, pointerPos, uiCamera))
            {
                Close();
            }
        }
    }

    public void Open()
    {
        // isOpen = true; // ここで即座にtrueにすると、ボタンを押した瞬間のクリック判定で閉じてしまう可能性がある
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveTo(endPos.position));
        StartCoroutine(SetIsOpenDelay(true)); // 1フレーム待ってからフラグを立てる
        PlayClickSE();
    }

    public void Close()
    {
        isOpen = false;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveTo(startPos.position));
        PlayClickSE();
    }

    private IEnumerator SetIsOpenDelay(bool open)
    {
        yield return null;
        isOpen = open;
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

    private void PlayClickSE()
    {
        if (clickAudioSource == null) return;
        if (clickAudioSource.isPlaying) clickAudioSource.Stop();
        clickAudioSource.Play();
    }
}
