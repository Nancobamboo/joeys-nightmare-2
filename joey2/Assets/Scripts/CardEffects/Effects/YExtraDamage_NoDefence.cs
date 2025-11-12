// Scripts/CardEffects/Effects/YExtraDamage_NoDefence.cs
using System.Collections;
using UnityEngine;

public class YExtraDamage_NoDefence : YDefaultEffect
{
    public int baseExtra;

    public YExtraDamage_NoDefence(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.ExtraDamage_NoDefence;
    }


    public override int GetEffectValue(EEffectType effectType)
    {
        if (effectType == EEffectType.Damage)
        {
            if (!JoeyGameControl.Instance.HasBagCard(ECardType.defence))
            {
                return baseExtra;
            }
            return 0;
        }
        return base.GetEffectValue(effectType);
    }
}

