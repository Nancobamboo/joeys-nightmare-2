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
        if (buffType == EBuffType.Counter)
        {
            int envIndex = CardControl.EnvIndex;
            if (JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                value--;
                if (value == 0)
                {
                    if (CardControl.CardData != null)
                    {
                        value = 5;
                        int attack = CardControl.CardData.currentAttack;
                        // Queue action to trigger
                        JoeyGameControl.Instance.QueueAction(EActionId.TakePlayerNoDefenceDamage, attack, EVFXName.VFX_Shouji);
                    }
                }
            }
        }
        return value;
    }
}

