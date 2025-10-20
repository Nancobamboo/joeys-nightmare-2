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

    // Start is called before the first frame update
    void Start()
    {
        playerData = GetComponent<PlayerData>();
        UpdateLibrary();
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    public void UpdateLibrary()
    {
        foreach (var item in playerData.playerDataDict)
        {
            if (item.Value > 0 && item.Key != "coin")
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, libraryPanel);
            }
        }
        
    }

    public void UpdateDeck()
    {

    }


}
