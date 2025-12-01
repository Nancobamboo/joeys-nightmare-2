// Scripts/CardEffects/YBareHands.cs
using System.Collections.Generic;
using UnityEngine;

public class YBareHands : YCardEffect
{
    public YBareHands()
    {
        Id = ECardEffectId.BareHands;
    }

    public override float OnUseFinished()
    {
        // if (CardControl != null && CardControl.gameObject != null)
        // {
        // 	var vfxNames = new List<EVFXName> { };
        // 	float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife);
        // 	return maxDelayTime;
        // }
        return 0f;
    }
}

