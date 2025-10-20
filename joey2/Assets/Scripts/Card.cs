using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public string id;
    public string name;
    public string desc;
    public  Card(string _id, string _name, string _desc)
    {
        this.id = _id;
        this.name = _name;
        this.desc = _desc;
    }

}


public class EnemyCard : Card
{
    public int hp;
    public int current_hp;
    public int attack;
    public int current_attack;

    public  EnemyCard(string _id, string _name, string _desc, int _hp, int _attack) : base(_id, _name, _desc)
    {
        this.hp = _hp;
        this.current_hp = _hp;
        this.attack = _attack;
        this.current_attack = _attack;
    }


}


public class ItemCard : Card
{
    public int attack;
    public int current_attack;
    public int heal;
    public int current_heal;
    public int price;
    public int current_price;
    public  ItemCard(string _id, string _name, string _desc, int _attack, int _heal, int _price) : base(_id, _name, _desc)
    {
        this.attack = _attack;
        this.current_attack = _attack;
        this.heal = _heal;
        this.current_heal = _heal;
        this.price = _price;
        this.current_price = _price;
    }
}


