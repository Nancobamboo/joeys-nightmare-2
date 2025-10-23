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
        foreach (var kv in itemData.libraryItemDict)
        {
            var ids = kv.Value;
            if (ids == null || ids.Count == 0) continue;

            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (!store.cardDict.ContainsKey(id)) continue;

                GameObject newCard = GameObject.Instantiate(cardPrefab, libraryPanel);
                newCard.GetComponent<CardDisplay>().card = store.cardDict[id];
                newCard.GetComponent<CardDisplay>().ShowCard();
            }
        }
        
    }

    public void UpdateDeck()
    {
        EnsureLoaded();
        foreach (var kv in itemData.deckItemDict)
        {
            var ids = kv.Value;
            if (ids == null || ids.Count == 0) continue;

            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (!store.cardDict.ContainsKey(id)) continue;

                GameObject newCard = GameObject.Instantiate(deckPrefab, deckPanel);
                newCard.GetComponent<CardDisplay>().card = store.cardDict[id];
                newCard.GetComponent<CardDisplay>().ShowCard();
            }
        }
    }


    public void UpdateCard(CardState _state, string _id, GameObject cardGO)
    {
        EnsureLoaded();

        if (!store.cardDict.ContainsKey(_id))
        {
            Debug.LogWarning("未知卡牌 id: " + _id);
            return;
        }
        string type = store.cardDict[_id].type;

        if (_state == CardState.Library){
            // 从 library 移除一张
            if (itemData.libraryItemDict.TryGetValue(type, out var libraryList))
            {
                libraryList.Remove(_id); // 只移除一个匹配项
            }
            
            // 添加到 deck
            if (!itemData.deckItemDict.TryGetValue(type, out var deckList))
            {
                deckList = new List<string>();
                itemData.deckItemDict[type] = deckList;
            }
            deckList.Add(_id);
            // 同步 UI 与状态
            if (cardGO != null)
            {
                cardGO.transform.SetParent(deckPanel, false);
                var cc = cardGO.GetComponent<ClickCard>();
                if (cc != null) cc.state = CardState.Deck;
            }
        }
        else if (_state == CardState.Deck){
            // 从 deck 移除一张
            if (itemData.deckItemDict.TryGetValue(type, out var deckList))
            {
                deckList.Remove(_id); // 只移除一个匹配项
            }

            // 添加回 library
            if (!itemData.libraryItemDict.TryGetValue(type, out var libraryList))
            {
                libraryList = new List<string>();
                itemData.libraryItemDict[type] = libraryList;
            }
            libraryList.Add(_id);

            // 同步 UI 与状态
            if (cardGO != null)
            {
                cardGO.transform.SetParent(libraryPanel, false);
                var cc = cardGO.GetComponent<ClickCard>();
                if (cc != null) cc.state = CardState.Library;
            }
        }
    }
}
