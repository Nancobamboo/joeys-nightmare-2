using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPack : MonoBehaviour
{


    public GameObject cardPrefab;
    public GameObject pool;
    List<GameObject> cardList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnClickOpen()
    {
        ClearPool();
        GData.Instance.LoadAll();
        // if (playerData.playerDataDict["coin"] >= 10 )
        // {
        //     playerData.playerDataDict["coin"] -= 10;
        // }
        // else
        // {
        //     Debug.Log("金币不足");
        //     return;
        // }
        for (int i = 0; i < 5; i++)
        {
            GameObject newCard = GameObject.Instantiate(cardPrefab, pool.transform);
            var cd = newCard.GetComponent<CardDisplay>();
            var c = GData.Instance.RandomCard();
            if (c == null)
            {
                Debug.LogWarning("随机到空卡，检查 Store.cardData 是否已绑定并成功加载");
                Destroy(newCard);
                continue;
            }
            cd.card = c;
            cd.ShowCard();
            cardList.Add(newCard);
        }
        Debug.Log("当前 cardList 数量: " + cardList.Count);
        // Debug.Log("当前玩家金币数: " + (playerData.playerDataDict.ContainsKey("coin") ? playerData.playerDataDict["coin"] : 0));
        SaveLibraryData();
        GData.Instance.SaveAll();
    }

    public void ClearPool()
    {
        foreach (var card in cardList)
        {
            Destroy(card);
        }
        cardList.Clear();
    }

    public void SaveLibraryData()
    {
        foreach (var card in cardList)
        {
            var display = card.GetComponent<CardDisplay>();
            if (display == null || display.card == null) continue;

            string id = display.card.id;
            string type = (display.card is EnemyCard) ? "enemy"
                : (display.card is ItemCard) ? "item"
                : "unknown";

            if (!GData.Instance.LibraryItemDict.TryGetValue(type, out var list))
            {
                list = new List<string>();
                GData.Instance.LibraryItemDict[type] = list;
            }
            list.Add(id);
        }
    }




}
