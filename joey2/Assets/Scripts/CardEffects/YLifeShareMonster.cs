using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YLifeShareMonster : YDefaultEffect
{
    private bool m_IsFirstTrigger = true;

    public YLifeShareMonster()
    {
        Id = ECardEffectId.LifeShareMonster;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 2);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter)
        {
            int envIndex = CardControl.EnvIndex;

            if (JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                if (m_IsFirstTrigger)
                {
                    ShareHealthWithPlayer();
                    m_IsFirstTrigger = false;
                    return 2;
                }
                else
                {
                    value--;
                    if (value <= 0)
                    {
                        ShareHealthWithPlayer();
                        return 2;
                    }
                }
            }

        }
        return value;
    }

    private void ShareHealthWithPlayer()
    {
        if (CardControl == null || CardControl.CardType != ECardType.monster)
        {
            return;
        }

        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        if (playerData == null)
        {
            return;
        }

        int monsterHealth = CardControl.CardData.currentHealth;
        int playerHealth = playerData.playerHealth;
        int averageHealth = (monsterHealth + playerHealth) / 2;

        CardControl.CardData.currentHealth = averageHealth;
        CardControl.RefreshCard();

        YActionSystem.Instance.DispatchAction(EActionId.LifeShareSetPlayerHealth, averageHealth);
    }
}

public partial class UIGamePhaseControl
{
    void LifeShareSetPlayerHealth(object[] paraArray)
    {
        int targetHealth = (int)paraArray[0];
        int currentHealth = m_DataJoeyPlayer.playerHealth;
        int delta = targetHealth - currentHealth;

        if (delta != 0)
        {
            ApplyPlayerHealthChange(delta, delta > 0);
        }
    }
}

