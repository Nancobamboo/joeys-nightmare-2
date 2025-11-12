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

    public override float UseAttack()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife, 0.65f);
        }
        return base.UseAttack();
    }


    public override float UseDefence()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
            float delayTime = Random.Range(0.5f, 1.5f);
            CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.CardLife, delayTime);
        }
        return base.UseDefence();
    }

    public override float OnDealDamage()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float delayTime = Random.Range(0.5f, 1.5f);
            CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife, delayTime);
        }
        return base.OnDealDamage();
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            switch (effectType)
            {
                case EEffectType.Boom:
                    var boomVfxNames = new List<EVFXName> { EVFXName.VFX_boom };
                    CardControl.PlayVFX(boomVfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife, 0.65f);
                    SFX.PlayAudio("Audio/SFX/Battle/boom", 1.0f, 0f);
                    return 0.65f;
                case EEffectType.Electric:
                    PlayElectricEffectAsync().Forget();
                    return 1.65f; // 雷弹特效时间(0.65f) + 延迟(0.5f) + 默认特效平均时间(0.5f)
                default:
                    var vfxNames = new List<EVFXName> { EVFXName.VFX_Shouji };
                    float delayTime = Random.Range(0.5f, 1.5f);
                    CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife, delayTime);
                    return delayTime;
            }
        }
        return base.OnTakeDamage(effectType);
    }
    private async UniTaskVoid PlayElectricEffectAsync()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            // 先播放雷弹特效
            var electricVfxNames = new List<EVFXName> { EVFXName.VFX_LeiDan };
            CardControl.PlayVFX(electricVfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife, 0.65f);
            await UniTask.WaitForSeconds(0.65f);
            // 执行default分支的特效
            var damageVfxNames = new List<EVFXName> { EVFXName.VFX_Shouji };
            float delayTime = Random.Range(0.5f, 1.5f);
            CardControl.PlayVFX(damageVfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife, delayTime);
        }
    }
    public override float OnDead()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_feitian, EVFXLife.SelfLife, 0f);
        }
        return base.OnDead();
    }
}

