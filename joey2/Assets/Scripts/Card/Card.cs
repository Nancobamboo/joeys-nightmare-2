using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    Default,Active,Inactive,Used
}

public enum CardPosition
{
    Default,Deck,Sell,Buy,Env,Bag,Used
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
    public int currentAttack;
    public int defence;
    public int currentDefence;
    public int health;
    public int currentHealth;
    public int price;
    public int currentPrice;
    public int stars;
    public CardState state;
    public CardState lastState;
    public CardPosition position;
    public CardPosition lastPosition;
    public List<string> effectIds = new List<string>();

    public  Card(string _id, string _type, string _cardImage, string _cardName, string _description, int _attack, int _defence, int _health, int _price, int _stars, List<string> _effectIds)
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
            this.iconType = "Art/UI/icon_item";
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
        this.currentAttack = _attack;
        this.defence = _defence;
        this.currentDefence = _defence;
        this.health = _health;
        this.currentHealth = _health;
        this.price = _price;
        this.currentPrice = _price;
        this.stars = _stars;
        if (_type == "other")
        {
            this.cardFrame = "Art/UI/bg_card_character";
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
        this.effectIds = _effectIds;
        this.state = CardState.Default;
        this.lastState = CardState.Default;
        this.position = CardPosition.Default;
        this.lastPosition = CardPosition.Default;
    }
    public void SetState(CardState state)
    {
        if (state != this.state)
        {
            this.lastState = this.state;
            this.state = state;
        }
    }
    public void SetPosition(CardPosition position)
    {
        if (position != this.position)
        {
            this.lastPosition = this.position;
            this.position = position;
        }
    }
    public Card Clone()
    {
        var c =  new Card(id, type, cardImage, cardName, description, attack, defence, health, price, stars, effectIds);
        return c;
    }
}
