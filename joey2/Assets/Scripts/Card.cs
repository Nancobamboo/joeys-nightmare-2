using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class Card : IData
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
    public string effectId;
    public int UniqueId;

    public Card()
    {

    }

    public Card(string _id, string _type, string _cardImage, string _cardName, string _description, int _attack, int _defence, int _health, int _price, int _stars, string _effectId)
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
            this.cardFrame = "Art/UI/card_bg_end";
        }
        else
        {
            if (_stars == 3)
            {
                this.cardFrame = "Art/UI/card_bg_boss";
            }
            else
            {
                this.cardFrame = "Art/UI/card_bg_normal";
            }
        }
        this.effectId = _effectId;
        this.UniqueId = 0;
    }

    public Card Clone()
    {
        var c = new Card(id, type, cardImage, cardName, description, attack, defence, health, price, stars, effectId);
        return c;
    }

    public ECardType GetCardType()
    {
        return (ECardType)System.Enum.Parse(typeof(ECardType), type);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
    }

    public void ParseEffectId(string effectIdStr, out string effectId, out int effectValue)
    {
        effectId = effectIdStr;
        effectValue = 0;

        if (string.IsNullOrEmpty(effectIdStr))
        {
            return;
        }

        string[] parts = effectIdStr.Split(':');
        if (parts.Length > 0)
        {
            effectId = parts[0].Trim();
        }
        if (parts.Length > 1)
        {
            if (int.TryParse(parts[1].Trim(), out int value))
            {
                effectValue = value;
            }
        }
    }

    public void LoadFromJson(JObject jobject)
    {
        id = (string)jobject["id"];
        type = (string)jobject["type"];
        iconType = (string)jobject["iconType"];
        cardImage = (string)jobject["cardImage"];
        cardFrame = (string)jobject["cardFrame"];
        cardName = (string)jobject["cardName"];
        description = (string)jobject["description"];
        attack = (int)jobject["attack"];
        currentAttack = (int)jobject["currentAttack"];
        defence = (int)jobject["defence"];
        currentDefence = (int)jobject["currentDefence"];
        health = (int)jobject["health"];
        currentHealth = (int)jobject["currentHealth"];
        price = (int)jobject["price"];
        currentPrice = (int)jobject["currentPrice"];
        stars = (int)jobject["stars"];
        effectId = (string)jobject["effectId"];
        UniqueId = (int)jobject["UniqueId"];
    }

    public void SaveToJson(JObject jobject)
    {
        jobject.Add("id", id);
        jobject.Add("type", type);
        jobject.Add("iconType", iconType);
        jobject.Add("cardImage", cardImage);
        jobject.Add("cardFrame", cardFrame);
        jobject.Add("cardName", cardName);
        jobject.Add("description", description);
        jobject.Add("attack", attack);
        jobject.Add("currentAttack", currentAttack);
        jobject.Add("defence", defence);
        jobject.Add("currentDefence", currentDefence);
        jobject.Add("health", health);
        jobject.Add("currentHealth", currentHealth);
        jobject.Add("price", price);
        jobject.Add("currentPrice", currentPrice);
        jobject.Add("stars", stars);
        jobject.Add("effectId", effectId);
        jobject.Add("UniqueId", UniqueId);
    }
}
