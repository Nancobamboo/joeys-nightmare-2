using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public string id;
    public string type;
    public string iconType;
    public string cardImage;
    public string cardFrame;
    public string cardName;
    public string description;
    public int attack;
    public int defence;
    public int health;
    public int price;
    public int stars;

    public  Card(string _id, string _type, string _cardImage, string _cardName, string _description, int _attack, int _defence, int _health, int _price, int _stars)
    {
        this.id = _id;
        this.type = _type;
        this.cardImage = _cardImage;
        this.cardName = _cardName;
        if (_type == "defence")
        {
            this.iconType = "Assets/Art/UI/icon_defense.png";
        }
        else if (_type == "attack")
        {
            this.iconType = "Assets/Art/UI/icon_attack.png";
        }
        else if (_type == "skill")
        {
            this.iconType = "Assets/Art/UI/icon_skill.png";
        }
        else if (_type == "item")
        {
            this.iconType = "Assets/Art/UI/icon_items.png";
        }
        else if (_type == "monster")
        {
            this.iconType = "Assets/Art/UI/icon_monster.png";
        }
        else
        {
            Debug.LogError("Card type is not valid: " + _type);
            this.iconType = null;
        }
        this.description = _description;
        this.attack = _attack;
        this.defence = _defence;
        this.health = _health;
        this.price = _price;
        this.stars = _stars;
        if (_type == "other")
        {
            this.cardFrame = "Assets/Art/UI/bg_card_golden.png";
        }
        else
        {
            if (_stars == 1)
            {
                this.cardFrame = "Assets/Art/UI/bg_card_write.png";
            }
            else if (_stars == 2)
            {
                this.cardFrame = "Assets/Art/UI/bg_card_silver.png";
            }
            else if (_stars == 3)
            {
                this.cardFrame = "Assets/Art/UI/bg_card_golden.png";
            }
            else 
            {
                Debug.LogError("Card stars is not valid: " + _stars);
                this.cardFrame = null;
            }
        }


    }

}



