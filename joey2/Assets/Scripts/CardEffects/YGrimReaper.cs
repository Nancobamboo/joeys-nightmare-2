using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class YGrimReaper : YDefaultEffect
{
    // 吸血数值
    private const int LIFESTEAL_AMOUNT = 5;
    // Boss卡牌ID映射
    private static readonly Dictionary<int, string> BossCardIds = new Dictionary<int, string>
    {
        { 0, "5027" }, // turkey boss: 大turkey霸
        { 1, "5034" }, // donkey boss: 果老Donkey
        { 2, "5019" }  // monkey boss: 大坏monkey
    };


    public YGrimReaper()
    {
        Id = ECardEffectId.GrimReaper;
    }

    // 造成伤害时触发吸血
    public override float OnDealDamage()
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            // 恢复5点生命值
            Card cardData = CardControl.CardData;
            cardData.currentHealth += LIFESTEAL_AMOUNT;
            if (cardData.currentHealth > cardData.health)
            {
                cardData.currentHealth = cardData.health;
            }
            CardControl.RefreshCard();
        }
        return base.OnDealDamage();
    }

    // 离场时召唤随机关卡的boss
    public override float OnDead()
    {
        if (CardControl != null)
        {
            if (ControlUtil.IsRandomSucceed(33))
            {
                YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "5027");
            }
            else if (ControlUtil.IsRandomSucceed(50))
            {
                YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "5034");
            }
            else
            {
                YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "5019");
            }
        }
        return base.OnDead();
    }

}

