// Scripts/CardEffects/Effects/YExtraDamage_NoDefence.cs
using System.Collections;
using UnityEngine;

public class YExtraDamage_NoDefence : YCardEffect
{
    public int baseExtra;

    public YExtraDamage_NoDefence(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.ExtraDamage_NoDefence;
    }

    public override void UseSkill()
    {
        base.UseSkill();
    }

    public override int GetEffectValue(EEffectType effectType)
    {
        if (effectType == EEffectType.Damage)
        {
            return baseExtra;
        }
        return base.GetEffectValue(effectType);
    }
}

