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

                CreateCard(id, CardState.Library);
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

                CreateCard(id, CardState.Deck);
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

            itemData.libraryItemDict[type].Remove(_id);
            // itemData.deckItemDict[type].Insert(0, _id);
            itemData.deckItemDict[type].Add(_id);
            CreateCard(_id, CardState.Deck);
            if (cardGO != null) Destroy(cardGO);
        }
        else if (_state == CardState.Deck){
            // 从 deck 移除一张
            itemData.deckItemDict[type].Remove(_id);
            itemData.libraryItemDict[type].Add(_id);
            CreateCard(_id, CardState.Library);
            if (cardGO != null) Destroy(cardGO);

        }
        itemData.SaveData();
    }


    public void CreateCard(string _id, CardState _state)
    {
        Transform targetPanel =null;
        GameObject targetPrefab=null;

        if (_state == CardState.Library)
        {
            targetPanel = libraryPanel;
            targetPrefab = cardPrefab;
        }
        else if (_state == CardState.Deck)
        {
            targetPanel = deckPanel;
            targetPrefab = deckPrefab;
        }

        GameObject newCard = GameObject.Instantiate(targetPrefab, targetPanel);
        newCard.transform.SetAsFirstSibling();
        newCard.GetComponent<CardDisplay>().card = store.cardDict[_id];
        newCard.GetComponent<CardDisplay>().ShowCard();
        var click = newCard.GetComponent<ClickCard>();
        if (click != null) click.state = _state;

    }
}
