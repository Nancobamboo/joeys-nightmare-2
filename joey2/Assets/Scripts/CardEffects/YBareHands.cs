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
            return 0.35f;
        }
        return base.OnDealDamage();
    }

    public override float OnUseFinished()
    {
        // Barehanded card should not be removed (永不消耗)
        // Don't play the drop animation
        return 0f;
    }
}

