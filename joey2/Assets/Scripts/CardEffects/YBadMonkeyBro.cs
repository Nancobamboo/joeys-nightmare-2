using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 兄贵Monkey的兄弟：
/// - 仅效果：倒计时10：召唤一个亲戚（5045/5046 随机）
/// </summary>
public class YBadMonkeyBro : YDefaultEffect
{
    private const string BRO_ID = "5045";
    private const string SIS_ID = "5046";
    private readonly int m_CounterReset;

    /// <param name="effectValue">倒计时回合数（来自 card_info: BadMonkeyBro:xx）</param>
    public YBadMonkeyBro(int effectValue) : base()
    {
        Id = ECardEffectId.BadMonkeyBro;
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

                    // 随机召唤 5045/5046（亲戚）
                    string summonId = Random.Range(0, 2) == 0 ? BRO_ID : SIS_ID;

                    // 尽量避免召唤到自己所在列，避免把自己盖住导致倒计时停摆
                    int envCount = JoeyGameControl.Instance.GetEnvPanelCount();
                    int targetEnvIndex = envIndex;
                    if (envCount > 1)
                    {
                        int r = Random.Range(0, envCount - 1);
                        targetEnvIndex = r >= envIndex ? r + 1 : r;
                    }

                    JoeyGameControl.Instance.QueueAction(EActionId.AddCardToSpecifiedEnv, CardControl, summonId, targetEnvIndex);
                }
            }
            return value;
        }

        return base.OnBuffValueChange(buffType, value);
    }
}


