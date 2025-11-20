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
        if (DataSystem.Instance.HasRelic(ERelicType.LifeSteal))
        {
            YActionSystem.Instance.DispatchAction(EActionId.AppHp, 1);
        }
        return base.UseAttack();
    }

    public override float UseDefence(bool isOverflow = false)
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames;
            if (isOverflow)
            {
                vfxNames = new List<EVFXName> { EVFXName.VFX_Dunsui };
            }
            else
            {
                vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
            }
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.CardLife);
            SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
            return maxDelayTime > 0f ? maxDelayTime : base.UseDefence(isOverflow);
        }
        return base.UseDefence(isOverflow);
    }

    public override float OnDealDamage()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            ECardAnimName animName = CardControl.CardType == ECardType.monster ? ECardAnimName.UI_Carditem_guaiwugongji : ECardAnimName.UI_Carditem_gongji;
            float maxDelayTime = CardControl.PlayVFX(vfxNames, animName, EVFXLife.CardLife);
            return maxDelayTime > 0f ? maxDelayTime : base.OnDealDamage();
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
                    float boomDelayTime = CardControl.PlayVFX(boomVfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife);
                    SFX.PlayAudio("Audio/SFX/Battle/boom", 1.0f, 0f);
                    return 0;
                case EEffectType.Electric:
                    PlayElectricEffectAsync().Forget();
                    return 0f;
                default:
                    var vfxNames = new List<EVFXName> { EVFXName.VFX_Shouji };
                    float shoujiDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.SelfLife);
                    SFX.PlayAudio("Audio/SFX/Battle/MonsterOnAttack", 1.0f, 0f);
                    return shoujiDelayTime;
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
            CardControl.PlayVFX(electricVfxNames, ECardAnimName.Idle, EVFXLife.CardLife);
            SFX.PlayAudio("Audio/SFX/Battle/electric", 1.0f, 0f);
            await UniTask.WaitForSeconds(0.65f);
            // 执行default分支的特效
            var damageVfxNames = new List<EVFXName> { EVFXName.VFX_Shouji };
            CardControl.PlayVFX(damageVfxNames, ECardAnimName.UI_Carditem_shouji, EVFXLife.CardLife);
            SFX.PlayAudio("Audio/SFX/Battle/MonsterOnAttack", 1.0f, 0f);
        }
    }
    public override float OnBeDying()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_feitian, EVFXLife.SelfLife);
            return maxDelayTime > 0f ? maxDelayTime : base.OnBeDying();
        }
        return base.OnBeDying();
    }

}

