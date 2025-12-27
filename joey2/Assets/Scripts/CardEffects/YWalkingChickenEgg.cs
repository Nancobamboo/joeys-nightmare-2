using System.Collections.Generic;
using UnityEngine;

public class YWalkingChickenEgg : YDefaultEffect
{
    public int baseExtra;

    public YWalkingChickenEgg(int baseExtra)
    {
        this.baseExtra = baseExtra;
        Id = ECardEffectId.WalkingChickenEgg;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 2);
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
                    // Queue action to trigger
                    JoeyGameControl.Instance.QueueAction(EActionId.AddCardToSpecifiedEnv, CardControl, baseExtra.ToString(), CardControl.EnvIndex);
                    // Remove from env and update other monsters
                    JoeyGameControl.Instance.RemoveEnvCardAndUpdate(envIndex, CardControl);
                }
            }
        }
        return value;
    }
}

