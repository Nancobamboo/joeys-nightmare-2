// Scripts/CardEffects/Effects/YDefaultEffect.cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class YDefaultEffect : YCardEffect
{
    public YDefaultEffect()
    {
        Id = (ECardEffectId)(-1);
    }

    public override void OnDealDamage()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float delayTime = Random.Range(0.5f, 1.5f);
            CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife, delayTime);
        }
    }

    public override void OnTakeDamage()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { EVFXName.VFX_Shouji };
            float delayTime = Random.Range(0.5f, 1.5f);
            CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife, delayTime);
        }
    }
}

