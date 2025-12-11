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


    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter)
        {
            int envIndex = CardControl.EnvIndex;
            if (JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                if (value == 0)
                {
                    if (CardControl.CardData != null)
                    {
                        // 对玩家造成伤害，参考 YGhost
                        int damage = CardControl.CardData.currentAttack;
                        YActionSystem.Instance.DispatchAction(EActionId.TakePlayerBoomDamage, damage, EVFXName.VFX_Shouji);

                        // 玩家扣 baseExtra 的钱
                        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
                        int currentCoin = playerData.Coin;
                        int newCoin = Mathf.Max(0, currentCoin - baseExtra);
                        DataSystem.Instance.AddCoin(-baseExtra);
                        
                        CardControl.Return();
                    }
                }
                value--;
            }
        }
        return value;
    }


}

