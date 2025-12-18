using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YNightmareLance : YCardEffect
{
    private bool m_CachedIsOneHealth = false;

    public YNightmareLance()
    {
        Id = ECardEffectId.NightmareLance;
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
                UpdateNightmareLanceDamage();
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
                    UpdateNightmareLanceDamage();
                }
            }
        }
        return value;
    }

    private void UpdateNightmareLanceDamage()
    {
        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        if (playerData != null && CardControl != null)
        {
            int currentAttack = CardControl.CardData?.currentAttack ?? 0;
            int maxHealth = playerData.playerMaxHealth;
            int extraDamage = maxHealth - currentAttack;

            if (extraDamage > 0)
            {
                CardControl.AddEffectValue(EEffectType.Damage, extraDamage);
            }
        }
    }
}

