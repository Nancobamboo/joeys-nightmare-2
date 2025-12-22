using System.Collections.Generic;
using UnityEngine;

public class YBlockFirstAttack : YDefaultEffect
{
    private bool m_HasBlockedFirstAttack = false;

    public YBlockFirstAttack()
    {
        Id = ECardEffectId.BlockFirstAttack;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl != null && CardControl.CardData != null && effectType == EEffectType.Damage && !m_HasBlockedFirstAttack)
        {
            // Block the first attack completely
            m_HasBlockedFirstAttack = true;
            
            // Play block visual effect
            List<EVFXName> vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.Idle, EVFXLife.SelfLife);
            
            // Return 0 damage
            return base.OnTakeDamage(effectType, 0);
        }
        return base.OnTakeDamage(effectType, damage);
    }
}

