using UnityEngine;

/// <summary>
/// 兄贵Monkey的姐妹：
/// - 仅效果：倒计时5：主动攻击一次（对玩家造成等同于自身攻击的无视护甲伤害）
/// </summary>
public class YBadMonkeySis : YDefaultEffect
{
    private readonly int m_CounterReset;

    /// <param name="effectValue">倒计时回合数（来自 card_info: BadMonkeySis:xx）</param>
    public YBadMonkeySis(int effectValue) : base()
    {
        Id = ECardEffectId.BadMonkeySis;
        m_CounterReset = Mathf.Max(1, effectValue);
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, m_CounterReset);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter)
        {
            int envIndex = CardControl.EnvIndex;
            if (JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                value--;
                if (value == 0)
                {
                    value = m_CounterReset;
                    if (CardControl.CardData != null)
                    {
                        int attack = CardControl.CardData.currentAttack;
                        JoeyGameControl.Instance.QueueAction(EActionId.TakePlayerNoDefenceDamage, attack, EVFXName.VFX_Shouji);
                    }
                }
            }
            return value;
        }

        return base.OnBuffValueChange(buffType, value);
    }
}


