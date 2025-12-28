// Scripts/CardEffects/Effects/YExtraDamage_HalfHealth.cs
using System.Collections;
using UnityEngine;

public class YExtraDamage_HalfHealth : YDefaultEffect
{
    public int baseExtra;
    private bool m_CachedHalfHealth = false;

    public YExtraDamage_HalfHealth(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.ExtraDamage_HalfHealth;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.UpdateByHpChange, 1);
            bool currentHalfHealth = JoeyGameControl.Instance.IsPlayerHalfHealth();
            m_CachedHalfHealth = currentHalfHealth;
            if (currentHalfHealth)
            {
                CardControl.AddEffectValue(EEffectType.Damage, baseExtra);
            }
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.UpdateByHpChange)
        {
            bool currentHalfHealth = JoeyGameControl.Instance.IsPlayerHalfHealth();
            if (currentHalfHealth != m_CachedHalfHealth)
            {
                m_CachedHalfHealth = currentHalfHealth;
                // Don't clear all effect values! Just add or subtract our bonus
                if (currentHalfHealth)
                {
                    // Player health dropped below 50%, add our bonus
                    CardControl.AddEffectValue(EEffectType.Damage, baseExtra);
                }
                else
                {
                    // Player health went above 50%, remove our bonus
                    CardControl.AddEffectValue(EEffectType.Damage, -baseExtra);
                }
            }
        }
        return value;
    }
}

