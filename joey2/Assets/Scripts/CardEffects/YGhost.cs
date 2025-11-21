using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YGhost : YDefaultEffect
{
    public YGhost()
    {
        Id = ECardEffectId.Ghost;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 5);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter && value == 0)
        {
            if (CardControl != null && CardControl.CardData != null)
            {
                int attack = CardControl.CardData.currentAttack;
                int envIndex = CardControl.EnvIndex;
                YActionSystem.Instance.DispatchAction(EActionId.TakePlayerDamage, attack, CardControl, envIndex);
            }
        }
        return value;
    }
}

