using UnityEngine;
using UnityEngine.EventSystems;

public class StageSelect_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int buttonIndex;   // 各ボタンごとに Inspector で設定

    public void OnPointerEnter(PointerEventData eventData)
    {
        StageSelect.Instance.ShowComment(buttonIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StageSelect.Instance.HideComment(buttonIndex);
    }
}
