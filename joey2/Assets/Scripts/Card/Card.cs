using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public string id;
    public string type;
    public string cardImage;
    public string cardFrame;
    public string cardName;
    public string iconType;//image path
    public string description;
    public int attack;
    public int defence;
    public int health;
    public int price;
    public int stars;

    public  Card(string _id, string _type, string _cardImage, string _cardFrame, string _cardName, string _iconType, string _description, int _attack, int _defence, int _health, int _price, int _stars)
    {
        this.id = _id;
        this.type = _type;
        this.cardImage = _cardImage;
        this.cardFrame = _cardFrame;
        this.cardName = _cardName;
        this.iconType = _iconType;
        this.description = _description;
        this.attack = _attack;
        this.defence = _defence;
        this.health = _health;
        this.price = _price;
        this.stars = _stars;
    }

}



