using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class open_pack : MonoBehaviour
{


    public GameObject cardPrefab;
    public GameObject pool;
    Store store;
    List<GameObject> cardList = new List<GameObject>();
    public PlayerData playerData;


    // Start is called before the first frame update
    void Start()
    {
        store = GetComponent<Store>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void handleCoin()
    {
        if (playerData.playerDataDict.TryGetValue("coin", out var coins))
        {
            if (coins >= 10)
            {
                playerData.playerDataDict["coin"] -= 10;
            }
            else
            {
                Debug.Log("金币不足");
                return;
            }
        }
        else
        {
            Debug.Log("没有金币数据");
            return;
        }
    }

    public void OnClickOpen()
    {
        ClearPool();
        handleCoin();
        for (int i = 0; i < 5; i++)
        {
            GameObject newCard = GameObject.Instantiate(cardPrefab, pool.transform);
            newCard.GetComponent<CardDisplay>().card = store.RandomCard();
            newCard.GetComponent<CardDisplay>().ShowCard();
            cardList.Add(newCard);
        }
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
