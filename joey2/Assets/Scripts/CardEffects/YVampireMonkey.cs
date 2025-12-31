using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YVampireMonkey : YDefaultEffect
{
    public YVampireMonkey()
    {
        Id = ECardEffectId.VampireMonkey;
    }

    public override float OnDealDamage()
    {
        if (CardControl != null)
        {
            Debug.Log("VampireMonkeyDealDamage on deal damage");
            YActionSystem.Instance.DispatchAction(EActionId.MonsterHealOnDealDamage, CardControl);
        }
        return base.OnDealDamage();
    }
}

public partial class UIGamePhaseControl
{
    public void VampireMonkeyDealDamage(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardType != ECardType.monster)
        {
            return;
        }
        Debug.Log("VampireMonkeyDealDamage handle logic");
        // Heal 3 HP, but don't exceed max health (no need to increase max health for healing)
        cardControl.CardData.currentHealth += 3;
        if (cardControl.CardData.currentHealth > cardControl.CardData.health)
        {
            cardControl.CardData.currentHealth = cardControl.CardData.health;
        }
        cardControl.RefreshCard();
        //cardControl.CallCardTakeDamage(3, EEffectType.Heal);
        // cardControl.CardEffect.AddEffectValue(EEffectType.Heal, 3);
    }
}