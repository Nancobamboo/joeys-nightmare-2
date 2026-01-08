// Scripts/CardEffects/YBareHands.cs
using System.Collections.Generic;
using UnityEngine;

public class YBareHands : YCardEffect
{
    private const int FIST_GROWTH_RATIO = 20; // percent
    private int m_AppliedGrowthBonus = 0;

    public YBareHands()
    {
        Id = ECardEffectId.BareHands;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);

        // Apply permanent bare-hands attack growth (from DataJoeyPlayer) as a relic effect value
        // so it persists through ClearTemporaryEffectValues and is visible in UI (green number).
        DataJoeyPlayer playerData = DataSystem.Instance != null ? DataSystem.Instance.GetDataJoeyPlayer() : null;
        int bonus = playerData != null ? playerData.bareHandsAttackBonus : 0;
        int delta = bonus - m_AppliedGrowthBonus;
        if (delta != 0)
        {
            AddRelicEffectValue(EEffectType.Damage, delta);
            m_AppliedGrowthBonus = bonus;
        }
    }


    public override float UseAttack()
    {
        return base.UseAttack();
    }

    public override float OnDealDamage()
    {
        // Play attack animation for Barehanded card
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            ECardAnimName animName = ECardAnimName.UI_Carditem_gongji;
            float maxDelayTime = CardControl.PlayVFX(vfxNames, animName, EVFXLife.CardLife);
            return 0.3f;
        }
        return base.OnDealDamage();
    }

    public override float OnKill()
    {
        // 拳王成长中：用赤手空拳(包含拳套等)每次击杀，有20%概率拳头攻击永久+1
        if (DataSystem.Instance != null && DataSystem.Instance.HasRelic(ERelicType.FistGrowth))
        {
            if (ControlUtil.IsRandomSucceed(FIST_GROWTH_RATIO))
            {
                DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
                playerData.bareHandsAttackBonus += 1;

                // 立刻把本次 +1 同步到“当前这张拳头/拳套卡”，这样本局后续攻击和UI立刻生效
                // 额外数值仍以 DataJoeyPlayer.bareHandsAttackBonus 为准，后续 SetData 时会统一应用
                AddRelicEffectValue(EEffectType.Damage, 1);
                m_AppliedGrowthBonus = playerData.bareHandsAttackBonus;
                CardControl?.RefreshCard();

                // 播放噬魂特效作为永久成长提示
                if (CardControl != null && CardControl.CacheTrans != null)
                {
                    JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, CardControl.CacheTrans, 1f);
                }
                return 1f;
            }
        }

        return base.OnKill();
    }

    public override float OnUseFinished(bool isSkip)
    {
        // Barehanded card should not be removed (永不消耗)
        // Don't play the drop animation
        return 0f;
    }

    public override float OnBecomeTopOfPile()
    {
        // BrassKnuckles relic effect: when bare hands becomes equipped, deal damage to random enemy
        if (DataSystem.Instance.HasRelic(ERelicType.BrassKnuckles))
        {
            if (CardControl != null && CardControl.CardData != null)
            {
                if (JoeyGameControl.Instance.HasEnemy())
                {
                    // Play attack animation
                    CardControl.PlayVFX(null, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);

                    // Get bare hands attack damage (including temporary bonuses)
                    int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);

                    // Dispatch action to attack random enemy with delay and clear temporary effects
                    JoeyGameControl.Instance.AddGlobalDelayCall(() =>
                    {
                        YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemyAndClearEffect, damage, 1, CardControl);
                    }, 0.3f);
                }
            }
        }
        return base.OnBecomeTopOfPile();
    }
}

