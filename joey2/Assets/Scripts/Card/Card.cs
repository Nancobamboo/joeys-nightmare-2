using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    Default,Deck,Sell,Buy,EnvActive,EnvInactive,BagActive,BagInactive
}


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
    public CardState state;

    public  Card(string _id, string _type, string _cardImage, string _cardName, string _description, int _attack, int _defence, int _health, int _price, int _stars)
    {
        this.id = _id;
        this.type = _type;
        this.cardImage = _cardImage;
        this.cardName = _cardName;
        if (_type == "defence")
        {
            this.iconType = "Art/UI/icon_defense";
        }
        else if (_type == "attack")
        {
            this.iconType = "Art/UI/icon_attack";
        }
        else if (_type == "skill")
        {
            this.iconType = "Art/UI/icon_skill";
        }
        else if (_type == "item")
        {
            this.iconType = "Art/UI/icon_items";
        }
        else if (_type == "monster")
        {
            this.iconType = "Art/UI/icon_monster";
        }
        else
        {
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
            this.cardFrame = "Art/UI/bg_card_golden";
        }
        else
        {
            if (_stars == 1)
            {
                this.cardFrame = "Art/UI/bg_card_write";
            }
            else if (_stars == 2)
            {
                this.cardFrame = "Art/UI/bg_card_silver";
            }
            else if (_stars == 3)
            {
                this.cardFrame = "Art/UI/bg_card_golden";
            }
            else 
            {
                Debug.LogError("Card stars is not valid: " + _stars);
                this.cardFrame = null;
            }
        }
        this.state = CardState.Default;

    }
    public Card Clone()
    {
        var c =  new Card(id, type, cardImage, cardName, description, attack, defence, health, price, stars);
        return c;
    }

}



