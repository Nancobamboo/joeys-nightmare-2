// Scripts/CardEffects/Effects/YStealMoney.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YStealMoney : YDefaultEffect
{
    public int baseExtra;
    private int m_StolenCoinAmount = 0;

    public YStealMoney(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.StealMoney;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 3);
        }
    }

    public override float OnDealDamage()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.StealCoin, this, baseExtra);
        }
        return base.OnDealDamage();
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
                    YActionSystem.Instance.DispatchAction(EActionId.EscapeMonkey, CardControl);
                }
            }
        }
        return value;
    }

    public override float OnDead()
    {
        if (CardControl != null && m_StolenCoinAmount > 0)
        {
            YActionSystem.Instance.DispatchAction(EActionId.ReturnCoin, this);
        }
        return base.OnDead();
    }

    public void AddStolenCoin(int amount)
    {
        m_StolenCoinAmount += amount;
    }

    public int GetStolenCoinAmount()
    {
        return m_StolenCoinAmount;
    }
}

