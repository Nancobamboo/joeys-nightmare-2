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
            var vfxNames = new List<string> { "VFX_boom" };
            CardControl.PlayVFX(vfxNames, animName: "UI_Carditem_gongji");

            float delayTime = Random.Range(0.5f, 1.5f);
            DelayStopVFX(delayTime).Forget();
        }
    }

    public override void OnTakeDamage()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<string> { "VFX_Shouji" };
            CardControl.PlayVFX(vfxNames, animName: "UI_Carditem_shouji");

            float delayTime = Random.Range(0.5f, 1.5f);
            DelayStopVFX(delayTime).Forget();
        }
    }
}

