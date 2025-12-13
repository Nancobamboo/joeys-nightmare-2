using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YPermanentAttackBoost : YDefaultEffect
{
    public int deltaPara;
    private bool m_HasBoostedAttack = false;

    public YPermanentAttackBoost(int deltaPara)
    {
        this.deltaPara = deltaPara;
        Id = ECardEffectId.PermanentAttackBoost;
    }

    public override float OnKill()
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            Card cardData = CardControl.CardData;
            cardData.currentAttack += deltaPara;
            CardControl.RefreshCard();
            // 播放 VFX_Shihun 特效，parent 是这个卡
            if (CardControl.CacheTrans != null)
            {
                JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, CardControl.CacheTrans, 1f);
            }
            m_HasBoostedAttack = true;
            return 1f;

        }
        return base.OnKill();
    }

    public override float OnUseFinished(bool IsSkip = false)
    {
        // 如果成功加了攻击并播放了特效，跳过弃牌动画
        if (m_HasBoostedAttack)
        {   
            m_HasBoostedAttack = false;
            return 0f;
        }
        // 否则正常播放弃牌动画
        return base.OnUseFinished(IsSkip);
    }

}

