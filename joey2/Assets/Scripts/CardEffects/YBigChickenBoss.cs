using System.Collections.Generic;
using UnityEngine;

public class YBigChickenBoss : YDefaultEffect
{
    public int baseExtra;

    public YBigChickenBoss(int baseExtra)
    {
        this.baseExtra = baseExtra;
        Id = ECardEffectId.BigChickenBoss;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl != null && effectType == EEffectType.Damage)
        {
            if (ControlUtil.IsRandomSucceed(20))
            {
                YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, baseExtra.ToString());
            }
        }
        return base.OnTakeDamage(effectType, damage);
    }
}

