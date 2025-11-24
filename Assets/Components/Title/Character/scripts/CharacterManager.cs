using UnityEngine;
using TMPro;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
    [Header("表情画像")]
    [SerializeField] private Sprite[] spriteList;
    public SpriteRenderer spriteRenderer;

    [Header("コメント関連")]
    [SerializeField] private string[] commentTexts;
    [SerializeField] private CanvasGroup commentObject;
    [SerializeField] private TextMeshProUGUI commentTextUI;

    [Header("設定")]
    [SerializeField] private float showTime = 3f;
    [SerializeField] private float fadeTime = 1f;

    private int currentIndex = 0;
    private Coroutine commentRoutine;

    void Start()
    {
        if (spriteList.Length == 0)
        {
            Debug.LogWarning("SpriteList に画像がセットされていません");
            return;
        }

        if (commentTexts.Length != spriteList.Length)
        {
            Debug.LogError("コメント数と画像数が一致していません");
        }

        currentIndex = Random.Range(0, spriteList.Length);
        spriteRenderer.sprite = spriteList[currentIndex];

        commentObject.alpha = 0;
        commentObject.gameObject.SetActive(false);
    }

    // ★ 外部から呼び出す関数
    public void TriggerComment()
    {
        if (commentRoutine != null) StopCoroutine(commentRoutine);
        commentRoutine = StartCoroutine(ShowComment());
    }

    private IEnumerator ShowComment()
    {
        commentObject.gameObject.SetActive(true);
        commentTextUI.text = commentTexts[currentIndex];

        // フェードIN
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            commentObject.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            yield return null;
        }
        commentObject.alpha = 1;

        yield return new WaitForSeconds(showTime);

        // フェードOUT
        t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            commentObject.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            yield return null;
        }
        commentObject.alpha = 0;
        commentObject.gameObject.SetActive(false);
        commentRoutine = null;
    }
}