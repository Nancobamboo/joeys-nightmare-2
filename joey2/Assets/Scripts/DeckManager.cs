using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{

    public Transform deckPanel;
    public Transform libraryPanel;
    public GameObject cardPrefab;
    public GameObject deckPrefab;
    private ItemData itemData;
    private Store store;

    // Start is called before the first frame update
    void Start()
    {
        itemData = GetComponent<ItemData>();
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
        if (itemData == null)
        {
			itemData = GetComponent<ItemData>();
			if (itemData == null)
			{
				itemData = FindObjectOfType<ItemData>();
			}
			if (itemData == null)
			{
				Debug.LogError("DeckManager: 找不到 ItemData 组件，请在场景中添加一个 ItemData。");
				return;
			}
        }

        store.EnsureLoaded();
        itemData.EnsureLoaded();

    }

    public void UpdateLibrary()
    {
        EnsureLoaded();
        foreach (var item in itemData.libraryItemDict)
        {
            if (item.Value > 0)
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
        foreach (var item in itemData.deckItemDict)
        {
            if (item.Value > 0)
            {
                GameObject newCard = GameObject.Instantiate(deckPrefab, deckPanel);
                newCard.GetComponent<CardDisplay>().card = store.cardDict[item.Key];
                newCard.GetComponent<CardDisplay>().ShowCard();
            }
        }
    }


}
