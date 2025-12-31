using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 监工Donkey：
/// - 倒计时X：对其它怪物造成1点伤害（触发它们的 OnTakeDamage，从而让 Donkey市长等“受击加攻”吃到效果）
/// - 配置来自 card_info.csv: WhipDonkey:1
/// </summary>
public class YWhipDonkey : YDefaultEffect
{
    private const int DAMAGE = 1;
    private readonly int m_CounterReset;

    /// <param name="effectValue">倒计时回合数（来自 card_info: WhipDonkey:xx）</param>
    public YWhipDonkey(int effectValue) : base()
    {
        Id = ECardEffectId.WhipDonkey;
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
                    JoeyGameControl.Instance.QueueAction(EActionId.WhipDonkeyDamage, CardControl, DAMAGE);
                }
            }
            return value;
        }

        return base.OnBuffValueChange(buffType, value);
    }
}

public partial class UIGamePhaseControl
{
    async void WhipDonkeyDamage(object[] paraArray)
    {
        UICardSimpleControl sourceCardControl = paraArray.Length > 0 ? paraArray[0] as UICardSimpleControl : null;
        int damage = 0;
        if (paraArray.Length > 1 && paraArray[1] is int v)
        {
            damage = v;
        }

        if (damage <= 0)
        {
            return;
        }

        // 对所有环境列的“最外层怪物”造成伤害（跳过自己）
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard == null || !lastCard.gameObject.activeSelf)
            {
                continue;
            }
            if (sourceCardControl != null && lastCard == sourceCardControl)
            {
                continue;
            }
            if (lastCard.CardType != ECardType.monster || lastCard.CardData == null || lastCard.CardData.currentHealth <= 0)
            {
                continue;
            }

            // 怪物打怪物：不应触发“反伤到玩家”
            // 注意：这里不要调用 GetOrCreateCardToken(sourceCardControl)，否则会把 CTS 留在 m_CardCtsDict 里导致回合推进卡死
            await DealDamageToEnvCard(lastCard, damage, i, EEffectType.Damage, cancellationToken: null, triggerThorns: false);
        }
    }
}


