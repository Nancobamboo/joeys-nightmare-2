using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPack : MonoBehaviour
{


    public GameObject cardPrefab;
    public GameObject pool;
    public Store store;
    List<GameObject> cardList = new List<GameObject>();
    public PlayerData playerData;


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
        playerData.LoadPlayerData();
        if (playerData.playerDataDict["coin"] >= 10 )
        {
            playerData.playerDataDict["coin"] -= 10;
        }
        else
        {
            Debug.Log("金币不足");
            return;
        }
        for (int i = 0; i < 5; i++)
        {
            GameObject newCard = GameObject.Instantiate(cardPrefab, pool.transform);
            var cd = newCard.GetComponent<CardDisplay>();
            var c = store.RandomCard();
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
        Debug.Log("当前玩家金币数: " + (playerData.playerDataDict.ContainsKey("coin") ? playerData.playerDataDict["coin"] : 0));
        SavePlayerData();
        playerData.SavePlayerData();
    }

    public void ClearPool()
    {
        foreach (var card in cardList)
        {
            Destroy(card);
        }
        cardList.Clear();
    }

    public void SavePlayerData()
    {
        foreach (var card in cardList)
        {
            string id = card.GetComponent<CardDisplay>().card.id;
            if (playerData.playerDataDict.TryGetValue(id, out var count))
            {
                playerData.playerDataDict[id] = count + 1;
            }
            else
            {
                playerData.playerDataDict[id] = 1;
            }        
        }
    }




}
