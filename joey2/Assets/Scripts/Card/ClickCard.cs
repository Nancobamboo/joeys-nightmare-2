using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardState
{
    Library,Deck,Hand,Battle,Discard,Trash
}


public class ClickCard : MonoBehaviour, IPointerDownHandler
{
    private PlayerData playerData;
    // Start is called before the first frame update
    void Start()
    {
        playerData = GameObject.Find("PlayerData").GetComponent<PlayerData>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void OnPointerDown(PointerEventData eventData)
    {

        if (state == CardState.Library)
        {
            // 从库中选择一张卡
        }
        else if (state == CardState.Deck)
        {
            // 从牌组中选择一张卡
        }
        else if (state == CardState.Hand)
        {
            // 从手牌中选择一张卡
        }
        else if (state == CardState.Battle)
        {
            // 从战斗区中选择一张卡
        }
        else if (state == CardState.Discard)
        {
            // 从弃牌堆中选择一张卡
        }
        else if (state == CardState.Trash)
        {
            // 从垃圾堆中选择一张卡
        }
        else
        {
            Debug.LogError("ClickCard: 未知的状态");
        }





    }

}
