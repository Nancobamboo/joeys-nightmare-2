using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{

    public Transform deckPanel;
    public Transform libraryPanel;
    public GameObject cardPrefab;
    public GameObject deckPrefab;
    private PlayerData playerData;
    private Store store;

    // Start is called before the first frame update
    void Start()
    {
        playerData = GetComponent<PlayerData>();
        store = GetComponent<Store>();
        UpdateLibrary();
        UpdateDeck();
    }

    // Update is called once per frame
    void Update()
    {

        
    }


    public void EnsureLoaded()
    {
        if (store == null)
        {
			store = GetComponent<Store>();
			if (store == null)
			{
				store = FindObjectOfType<Store>();
			}
			if (store == null)
			{
				Debug.LogError("DeckManager: 找不到 Store 组件，请在场景中添加一个 Store。");
				return;
			}
        }
        if (playerData == null)
        {
			playerData = GetComponent<PlayerData>();
			if (playerData == null)
			{
				playerData = FindObjectOfType<PlayerData>();
			}
			if (playerData == null)
			{
				Debug.LogError("DeckManager: 找不到 PlayerData 组件，请在场景中添加一个 PlayerData。");
				return;
			}
        }
        if (store.cardDict == null || store.cardDict.Count == 0)
        {
            store.LoadCards();
        }
        if (playerData.playerDataDict == null || playerData.playerDataDict.Count == 0)
        {
            playerData.LoadPlayerData();
        }
    }

    public void UpdateLibrary()
    {
        EnsureLoaded();
        foreach (var item in playerData.playerDataDict)
        {
            if (item.Value > 0 && item.Key != "coin")
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, libraryPanel);
                newCard.GetComponent<CardDisplay>().card = store.cardDict[item.Key];
                newCard.GetComponent<CardDisplay>().ShowCard();
            }
        }
        
    }

    public void UpdateDeck()
    {
        EnsureLoaded();
        foreach (var item in playerData.playerDataDict)
        {
            if (item.Value > 0 && item.Key != "coin")
            {
                GameObject newCard = GameObject.Instantiate(deckPrefab, deckPanel);
                newCard.GetComponent<CardDisplay>().card = store.cardDict[item.Key];
                newCard.GetComponent<CardDisplay>().ShowCard();
            }
        }
    }


}
