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
        cardControl.CardData.currentHealth += 3;
        cardControl.RefreshCard();
        //cardControl.CallCardTakeDamage(3, EEffectType.Heal);
        // cardControl.CardEffect.AddEffectValue(EEffectType.Heal, 3);
    }
}