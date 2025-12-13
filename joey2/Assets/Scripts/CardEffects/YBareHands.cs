// Scripts/CardEffects/YBareHands.cs
using System.Collections.Generic;
using UnityEngine;

public class YBareHands : YCardEffect
{
    public YBareHands()
    {
        Id = ECardEffectId.BareHands;
    }

    public override float OnDealDamage()
    {
        // Play attack animation for Barehanded card
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            ECardAnimName animName = ECardAnimName.UI_Carditem_gongji;
            float maxDelayTime = CardControl.PlayVFX(vfxNames, animName, EVFXLife.CardLife);
            return 0.3f;
        }
        return base.OnDealDamage();
    }

    public override float OnUseFinished(bool isSkip)
    {
        // Barehanded card should not be removed (永不消耗)
        // Don't play the drop animation
        return 0f;
    }

    public override float OnBecomeTopOfPile()
    {
        // BrassKnuckles relic effect: when bare hands becomes equipped, deal damage to random enemy
        if (DataSystem.Instance.HasRelic(ERelicType.BrassKnuckles))
        {
            if (CardControl != null && CardControl.CardData != null)
            {
                // Get bare hands attack damage
                int damage = CardControl.CardData.currentAttack;
                
                // Dispatch action to attack random enemy
                YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, 1);
                
                return 0.3f; // Delay time for effect
            }
        }
        return base.OnBecomeTopOfPile();
    }
}

