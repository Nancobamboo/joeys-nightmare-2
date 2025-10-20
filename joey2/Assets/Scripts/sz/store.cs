using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : MonoBehaviour
{
    public TextAsset cardData;
    public List<Card> cardList = new List<Card>();


    // Start is called before the first frame update
    void Start()
    {
        LoadCards();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadCards()
    {
        //name	id	type	desc	attack	hp	heal	price
        string[] lines = cardData.text.Split('\n');
        foreach (var line in lines)
        {

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] values = line.Split(',');
            if (values.Length < 5)
            {
                Debug.LogWarning("跳过不完整的行: " + line);
                continue;
            }

            if (values[0] == "name" || values[2] == "type") 
            {
                continue;
            }
            else if (values[2] == "enemy"){
                //create an enemy card
                string name = values[0];
                string id = values[1];
                string type = values[2];
                string desc = values[3];
                int attack = int.Parse(values[4]);
                int hp = int.Parse(values[5]);
                EnemyCard enemyCard = new EnemyCard(id, name, desc, attack, hp);
                cardList.Add(enemyCard);
                Debug.Log("Enemy card loaded: " + name);
            }
            else if (values[2] == "item"){
                //create an item card
                string name = values[0];
                string id = values[1];
                string type = values[2];
                string desc = values[3];
                int attack = int.Parse(values[4]);
                int heal = int.Parse(values[6]);
                int price = int.Parse(values[7]);
                ItemCard itemCard = new ItemCard(id, name, desc, attack, heal, price);
                cardList.Add(itemCard);
                Debug.Log("Item card loaded: " + name);

            }

        }
    }

    public Card RandomCard()
    {
        int randomIndex = Random.Range(0, cardList.Count);
        Card randomCard = cardList[randomIndex];
        return randomCard;
    }

}
