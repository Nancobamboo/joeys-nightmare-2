using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YBrokenShieldRedemption : YCardEffect
{
    public YBrokenShieldRedemption()
    {
        Id = ECardEffectId.BrokenShieldRedemption;
    }

    public override float UseDefence(bool isOverflow = false)
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.AddBlockDamagePhase, 1);

            return 0.3f;
        }
        return base.UseDefence(isOverflow);
    }
}

