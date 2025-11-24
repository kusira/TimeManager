using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterHitDetector : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private CharacterManager characterManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        characterManager.TriggerComment();
    }
}
