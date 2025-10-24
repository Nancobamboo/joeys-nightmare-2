using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardState
{
    Library,Deck,Enemy,BagSkill,BagItem
}


public class ClickCard : MonoBehaviour, IPointerDownHandler
{
    
    public CardState state = CardState.Library;
    private DeckManager deckManager;
    // Start is called before the first frame update
    void Start()
    {
        // itemData = GameObject.Find("ItemData").GetComponent<ItemData>();
        deckManager = GameObject.Find("DeckManager").GetComponent<DeckManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void OnPointerDown(PointerEventData eventData)
    {

        string id = this.GetComponent<CardDisplay>().card.id;
        if (state == CardState.Library || state == CardState.Deck)
        {
            deckManager.UpdateCard(state, id,this.gameObject);
        }
        else if (state == CardState.Enemy)
        {
            // 从敌人中选择一张卡
        }
        else if (state == CardState.BagSkill)
        {
            // 从技能背包中选择一张卡
        }
        else if (state == CardState.BagItem)
        {
            // 从物品背包中选择一张卡
        }
        else
        {
            Debug.LogError("ClickCard: 未知的状态");
        }





    }

}
