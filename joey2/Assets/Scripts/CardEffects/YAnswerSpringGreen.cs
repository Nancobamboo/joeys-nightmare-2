using System.Collections.Generic;
using UnityEngine;

public class YAnswerSpringGreen : YDefaultEffect
{
    public YAnswerSpringGreen()
    {
        Id = ECardEffectId.AnswerSpringGreen;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl != null && effectType == EEffectType.Damage)
        {
            YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponToEnv, CardControl);
        }
        return base.OnTakeDamage(effectType, damage);
    }
}

