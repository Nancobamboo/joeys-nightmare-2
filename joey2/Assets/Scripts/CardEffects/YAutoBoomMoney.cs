// Scripts/CardEffects/Effects/YAutoBoomMoney.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YAutoBoomMoney : YDefaultEffect
{
    public int baseExtra;

    public YAutoBoomMoney(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.AutoBoomMoney;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 7);
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
                    // Queue the action to trigger
                    JoeyGameControl.Instance.QueueAction(EActionId.TakePlayerBoomDamage, baseExtra, EVFXName.VFX_boom);
                    // Remove from env and update other monsters
                    JoeyGameControl.Instance.RemoveEnvCardAndUpdate(envIndex, CardControl);
                }
            }
        }
        return value;
    }
}

