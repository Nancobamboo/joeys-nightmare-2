using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{

    public Transform deckPanel;
    public Transform libraryPanel;
    public GameObject cardPrefab;
    public GameObject deckPrefab;

    // Start is called before the first frame update
    void Start()
    {
        GData.Instance.LoadAll();
        UpdateLibrary();
        UpdateDeck();
    }

    // Update is called once per frame
    void Update()
    {

        
    }


    public void UpdateLibrary()
    {
        GData.Instance.LoadAll();
        foreach (var kv in GData.Instance.LibraryItemDict)
        {
            var ids = kv.Value;
            if (ids == null || ids.Count == 0) continue;

            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (!GData.Instance.CardDict.ContainsKey(id)) continue;

                CreateCard(id, CardState.Library);
            }
        }
        
    }

    public void UpdateDeck()
    {
        GData.Instance.LoadAll();
        foreach (var kv in GData.Instance.DeckItemDict)
        {
            var ids = kv.Value;
            if (ids == null || ids.Count == 0) continue;

            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (!GData.Instance.CardDict.ContainsKey(id)) continue;

                CreateCard(id, CardState.Deck);
            }
        }
    }


    public void UpdateCard(CardState _state, string _id, GameObject cardGO)
    {
        GData.Instance.LoadAll();

        if (!GData.Instance.CardDict.ContainsKey(_id))
        {
            Debug.LogWarning("未知卡牌 id: " + _id);
            return;
        }
        string type = GData.Instance.CardDict[_id].type;

        if (_state == CardState.Library){
            // 从 library 移除一张

            GData.Instance.LibraryItemDict[type].Remove(_id);
            // itemData.deckItemDict[type].Insert(0, _id);
            GData.Instance.DeckItemDict[type].Add(_id);
            CreateCard(_id, CardState.Deck);
            if (cardGO != null) Destroy(cardGO);
        }
        else if (_state == CardState.Deck){
            // 从 deck 移除一张
            GData.Instance.DeckItemDict[type].Remove(_id);
            GData.Instance.LibraryItemDict[type].Add(_id);
            CreateCard(_id, CardState.Library);
            if (cardGO != null) Destroy(cardGO);

        }
        GData.Instance.SaveAll();
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
        newCard.GetComponent<CardDisplay>().card = GData.Instance.CardDict[_id];
        newCard.GetComponent<CardDisplay>().ShowCard();
        var click = newCard.GetComponent<ClickCard>();
        if (click != null) click.state = _state;

    }
}
