// Scripts/CardEffects/Effects/YExtraDamage_HalfHealth.cs
using System.Collections;
using UnityEngine;

public class YExtraDamage_HalfHealth : YDefaultEffect
{
    public int baseExtra;

    public YExtraDamage_HalfHealth(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.ExtraDamage_HalfHealth;
    }


    public override int GetEffectValue(EEffectType effectType)
    {
        if (effectType == EEffectType.Damage)
        {
            if (JoeyGameControl.Instance.IsPlayerHalfHealth())
            {
                return baseExtra;
            }
            return 0;
        }
        return base.GetEffectValue(effectType);
    }
}

