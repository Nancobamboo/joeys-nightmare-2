// Scripts/CardEffects/YMagicWand.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魔法杖卡牌效果：
/// 1. 攻击时视作施放技能，能被奥术宝珠等relic计数
/// 2. 永不消耗（和赤手空拳相同）
/// </summary>
public class YMagicWand : YCardEffect
{
    public YMagicWand()
    {
        Id = ECardEffectId.MagicWand;
    }

    public override float UseAttack()
    {
        // 魔法杖攻击时视作施放技能，触发相关relic效果（如奥术宝珠计数）
        YActionSystem.Instance.DispatchAction(EActionId.OnSkillCast);
        return base.UseAttack();
    }

    public override float OnDealDamage()
    {
        // Play attack animation for Magic Wand card
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
        // 魔法杖永不消耗（和赤手空拳相同）
        // 不播放掉落动画
        return 0f;
    }
}

