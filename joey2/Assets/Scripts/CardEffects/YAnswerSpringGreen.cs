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
        //if (effectType == EEffectType.Damage)
        {
            JoeyGameControl.Instance.AddGlobalDelayCall(() =>
            {
                YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponToEnv);
            }, 0.3f);
        }
        return base.OnTakeDamage(effectType, damage);
    }
}

