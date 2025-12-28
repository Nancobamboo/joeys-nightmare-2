// Scripts/CardEffects/Effects/YExtraDamage_NoDefence.cs
using System.Collections;
using UnityEngine;

public class YExtraDamage_NoDefence : YDefaultEffect
{
    public int baseExtra;
    private bool m_CachedNoDefence = false;

    public YExtraDamage_NoDefence(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.ExtraDamage_NoDefence;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.UpdateByDefenceCardNum, 1);
            bool currentNoDefence = !JoeyGameControl.Instance.HasBagCard(ECardType.defence);
            m_CachedNoDefence = currentNoDefence;
            if (currentNoDefence)
            {
                CardControl.AddEffectValue(EEffectType.Damage, baseExtra);
            }
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.UpdateByDefenceCardNum)
        {
            bool currentNoDefence = !JoeyGameControl.Instance.HasBagCard(ECardType.defence);
            if (currentNoDefence != m_CachedNoDefence)
            {
                m_CachedNoDefence = currentNoDefence;
                // Don't clear all effect values! Just add or subtract our bonus
                if (currentNoDefence)
                {
                    // No defence card, add our bonus
                    CardControl.AddEffectValue(EEffectType.Damage, baseExtra);
                }
                else
                {
                    // Defence card equipped, remove our bonus
                    CardControl.AddEffectValue(EEffectType.Damage, -baseExtra);
                }
            }
        }
        return value;
    }
}

