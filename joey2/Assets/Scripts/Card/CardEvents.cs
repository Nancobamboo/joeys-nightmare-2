using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class CardEvents : MonoBehaviour, IPointerDownHandler,IPointerEnterHandler, IPointerExitHandler
{
    public float zoomSize = 1.2f;

    public CardDisplay GetCardDisplay()
    {
        var cd = this.GetComponent<CardDisplay>();
        if (cd == null || cd.card == null)
        {
            Debug.LogError("ClickCard: CardDisplay 或 card 为空");
            return null;
        }
        return cd;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 只允许鼠标左键
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (PData.Instance.canOperate == false)
        {
            return;
        }

        var cd = GetCardDisplay();
        // Debug.Log($"CardClick: {id}");
        if (cd.card.state == CardState.Active)
        {
            Debug.Log($"CardClick: {cd.card.id}");
            GameEvents.RaiseCardClicked(cd.gameObject); // 广播全局事件
        }
        else
        {
            Debug.LogError("ClickCard: 未知的状态");
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var cd = GetCardDisplay();
        if (cd.card.position == CardPosition.Deck)
        {
            transform.localScale = new Vector3(zoomSize, zoomSize, 1.0f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var cd = GetCardDisplay();
        if (cd.card.position == CardPosition.Deck)
        {
            transform.localScale = Vector3.one;
        }
    }



}
