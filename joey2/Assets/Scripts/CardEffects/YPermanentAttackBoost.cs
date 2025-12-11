using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YPermanentAttackBoost : YDefaultEffect
{
    public int deltaPara;

    public YPermanentAttackBoost(int deltaPara)
    {
        this.deltaPara = deltaPara;
        Id = ECardEffectId.PermanentAttackBoost;
    }

    public override float OnKill()
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            Card cardData = CardControl.CardData;
            cardData.currentAttack += deltaPara;
            CardControl.RefreshCard();
        }
        return base.OnKill();
    }
}

