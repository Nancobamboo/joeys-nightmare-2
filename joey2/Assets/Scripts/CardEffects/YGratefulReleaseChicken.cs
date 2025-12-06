using System.Collections.Generic;
using UnityEngine;

public class YGratefulReleaseChicken : YDefaultEffect
{
    private bool m_HasBlockedFatalDamage = false;

    public YGratefulReleaseChicken()
    {
        Id = ECardEffectId.GratefulReleaseChicken;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl != null && CardControl.CardData != null && effectType == EEffectType.Damage && !m_HasBlockedFatalDamage)
        {
            int currentHealth = CardControl.CardData.currentHealth;
            if (damage >= currentHealth)
            {
                m_HasBlockedFatalDamage = true;
                CardControl.CardData.currentHealth = 1;
                CardControl.RefreshCard();
                return base.OnTakeDamage(effectType, 0);
            }
        }
        return base.OnTakeDamage(effectType, damage);
    }
}

