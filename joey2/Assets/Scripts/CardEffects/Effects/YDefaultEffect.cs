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

    public override void OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            switch (effectType)
            {
                case EEffectType.Boom:
                    var boomVfxNames = new List<EVFXName> { EVFXName.VFX_boom };
                    CardControl.PlayVFX(boomVfxNames, ECardAnimName.None, EVFXLife.SelfLife, 0.65f);
                    SFX.PlayAudio("Audio/SFX/Battle/boom", 1.0f, 0f);
                    break;
                default:
                    var vfxNames = new List<EVFXName> { EVFXName.VFX_Shouji };
                    float delayTime = Random.Range(0.5f, 1.5f);
                    CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife, delayTime);
                    break;
            }
        }
    }
}

