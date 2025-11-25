using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Components.Game; // StageManagerのために必要

public class StageSelect_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int buttonIndex;   // 各ボタンごとに Inspector で設定

    void Start()
    {
        var btn = GetComponent<Button>();
        var audio = GetComponent<AudioSource>();

        if (btn != null)
        {
            if (audio != null)
            {
                btn.onClick.AddListener(() => audio.Play());
            }

            // 到達した最大ステージより大きいインデックスのボタンは無効化する
            // (まだ到達していないステージ)
            int maxStage = StageManager.GetMaxReachedStage();
            if (buttonIndex > maxStage)
            {
                btn.interactable = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StageSelect.Instance.ShowComment(buttonIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StageSelect.Instance.HideComment(buttonIndex);
    }
}
