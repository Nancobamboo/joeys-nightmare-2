using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : MonoBehaviour
{
    public TextAsset cardData;
    public Dictionary<string, Card> cardDict = new Dictionary<string, Card>();

    void Awake()
    {
        // 比 Start 更早，避免 UI 先触发抽卡而数据未加载
        EnsureLoaded();
    }
    // Start is called before the first frame update
    void Start()
    {
        EnsureLoaded();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnsureLoaded()
    {
        if (cardDict.Count > 0) return;

        if (cardData == null)
        {
            Debug.LogError("Store.cardData 未绑定，无法加载卡牌数据");
            return;
        }

        LoadCards();

        if (cardDict.Count == 0)
        {
            Debug.LogError("加载完成但 cardDict 仍为空，请检查 card_info.csv 内容与分隔符是否为英文逗号");
        }
    }


    public void LoadCards()
    {
        //	id	name type	desc	attack	hp	heal	price
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

            if (values[0] == "id") 
            {
                continue;
            }
            else if (values[2] == "enemy"){
                //create an enemy card
                string id = values[0].Trim();
                string name = values[1].Trim();
                string type = values[2].Trim();
                string desc = values[3].Trim();
                int attack = int.Parse(values[4].Trim());
                int hp = int.Parse(values[5].Trim());
                EnemyCard enemyCard = new EnemyCard(id, name, desc, type, attack, hp);
                cardDict[id] = enemyCard;
            }
            else {
                //create an item card
                string id = values[0].Trim();
                string name = values[1].Trim();
                string type = values[2].Trim();
                string desc = values[3].Trim();
                int attack = int.Parse(values[4].Trim());
                int heal = int.Parse(values[6].Trim());
                int price = int.Parse(values[7].Trim());
                ItemCard itemCard = new ItemCard(id, name, desc, type, attack, heal, price);
                cardDict[id] = itemCard;

            }

        }
    }

    public Card RandomCard()
    {
        if (cardDict.Count == 0)
        {
            EnsureLoaded();
        }
        if (cardDict.Count == 0)
        {
            Debug.LogWarning("cardDict 为空，无法随机抽取卡牌");
            return null;
        }
        var values = new List<Card>(cardDict.Values);
        int randomIndex = Random.Range(0, values.Count);
        Card randomCard = values[randomIndex];
        return randomCard;
    }





}
