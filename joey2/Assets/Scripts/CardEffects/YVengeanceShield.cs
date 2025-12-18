using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YVengeanceShield : YCardEffect
{
    private bool m_CachedIsOneHealth = false;

    public YVengeanceShield()
    {
        Id = ECardEffectId.VengeanceShield;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.UpdateByHpChange, 1);
            DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
            bool currentIsOneHealth = playerData != null && playerData.playerHealth == 1;
            m_CachedIsOneHealth = currentIsOneHealth;
            if (currentIsOneHealth)
            {
                UpdateVengeanceShieldDefence();
            }
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.UpdateByHpChange)
        {
            DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
            bool currentIsOneHealth = playerData != null && playerData.playerHealth == 1;
            if (currentIsOneHealth != m_CachedIsOneHealth)
            {
                m_CachedIsOneHealth = currentIsOneHealth;
                CardControl.ClearEffectVlaue();
                if (currentIsOneHealth)
                {
                    UpdateVengeanceShieldDefence();
                }
            }
        }
        return value;
    }

    private void UpdateVengeanceShieldDefence()
    {
        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        if (playerData != null && CardControl != null)
        {
            int currentDefence = CardControl.CardData?.currentDefence ?? 0;
            int maxHealth = playerData.playerMaxHealth;
            int defenceValue = maxHealth / 2;
            int extraDefence = defenceValue - currentDefence;

            if (extraDefence > 0)
            {
                CardControl.AddEffectValue(EEffectType.Defence, extraDefence);
            }
        }
    }
}

