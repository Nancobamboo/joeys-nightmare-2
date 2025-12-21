using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Card : IData
{
    public string id;
    public string type;
    public string iconType;
    public string cardImage;
    public string cardBackground;
    public string cardFrame;
    public string cardName;
    public string description;
    public string baseDescription; // Store original description with placeholders
    private int attack;
    public int currentAttack
    {
        get { return attack; }
        set
        {
            if (attack != value)
            {
                attack = value;
                UpdateEnvCardDict();
            }
        }
    }
    private int defence;
    public int currentDefence
    {
        get { return defence; }
        set
        {
            if (defence != value)
            {
                defence = value;
                UpdateEnvCardDict();
            }
        }
    }
    public int health;
    public int currentHealth;
    public int price;
    public int stars;
    public string effectId;
    public int UniqueId;
    public int durability; // Track durability for cards like Knight Shield

    public Card()
    {

    }

    public Card(string _id, string _type, string _cardImage, string _cardBackground, string _cardName, string _description, int _attack, int _defence, int _health, int _price, int _stars, string _effectId)
    {
        this.id = _id;
        this.type = _type;
        this.cardImage = _cardImage;
        this.cardName = _cardName;
        this.cardBackground = _cardBackground;

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
        this.baseDescription = _description; // Store original description
        this.attack = _attack;
        this.attack = _attack;
        this.defence = _defence;
        this.defence = _defence;
        this.health = _health;
        this.currentHealth = _health;
        this.price = _price;
        this.stars = _stars;
        if (_stars == 3 & _type == "monster")
        {
            this.cardFrame = "Art/UI/card_bg_boss";
        }
        else if (_id == "6001")
        {
            this.cardFrame = "Art/UI/card_bg_end";
        }
        else
        {
            this.cardFrame = "Art/UI/card_bg_normal";
        }
        this.effectId = _effectId;
        this.UniqueId = 0;
    }

    public Card Clone()
    {
        var c = new Card(id, type, cardImage, cardBackground, cardName, description, attack, defence, health, price, stars, effectId);
        c.attack = attack;
        c.defence = defence;
        c.UniqueId = UniqueId;
        c.durability = durability;
        c.baseDescription = baseDescription;
        return c;
    }

    // Get description with placeholders replaced
    public string GetFormattedDescription()
    {
        if (string.IsNullOrEmpty(baseDescription))
        {
            return description;
        }
        
        string formattedDesc = baseDescription;
        
        // Replace durability placeholder
        if (formattedDesc.Contains("{durability}"))
        {
            formattedDesc = formattedDesc.Replace("{durability}", durability.ToString());
        }
        
        return formattedDesc;
    }

    public void SetAttack(int value)
    {
        attack = value;
    }

    public void SetDefence(int value)
    {
        defence = value;
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
        cardBackground = (string)jobject["cardBackground"];
        cardFrame = (string)jobject["cardFrame"];
        cardName = (string)jobject["cardName"];
        description = (string)jobject["description"];
        baseDescription = jobject["baseDescription"] != null ? (string)jobject["baseDescription"] : description;
        attack = (int)jobject["attack"];
        defence = (int)jobject["defence"];
        health = (int)jobject["health"];
        currentHealth = (int)jobject["currentHealth"];
        price = (int)jobject["price"];
        stars = (int)jobject["stars"];
        effectId = (string)jobject["effectId"];
        UniqueId = (int)jobject["UniqueId"];
        durability = jobject["durability"] != null ? (int)jobject["durability"] : 0;
    }

    private void UpdateEnvCardDict()
    {
        ECardType cardType = GetCardType();
        if (cardType == ECardType.attack || cardType == ECardType.defence)
        {
            DataJoeyPlayer dataJoeyPlayer = DataSystem.Instance.GetDataJoeyPlayer();
            dataJoeyPlayer.AddEnvCardDictData(id, this);
        }
    }

    public void SaveToJson(JObject jobject)
    {
        jobject.Add("id", id);
        jobject.Add("type", type);
        jobject.Add("iconType", iconType);
        jobject.Add("cardImage", cardImage);
        jobject.Add("cardBackground", cardBackground);
        jobject.Add("cardFrame", cardFrame);
        jobject.Add("cardName", cardName);
        jobject.Add("description", description);
        jobject.Add("baseDescription", baseDescription);
        jobject.Add("attack", attack);
        jobject.Add("defence", defence);
        jobject.Add("health", health);
        jobject.Add("currentHealth", currentHealth);
        jobject.Add("price", price);
        jobject.Add("stars", stars);
        jobject.Add("effectId", effectId);
        jobject.Add("UniqueId", UniqueId);
        jobject.Add("durability", durability);
    }
}
