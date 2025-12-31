using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YNightmareLance : YDefaultEffect
{
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
            UpdateNightmareLanceDamage();
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.UpdateByHpChange)
        {
            CardControl.ClearEffectVlaue();
            UpdateNightmareLanceDamage();
        }
        return value;
    }

    private void UpdateNightmareLanceDamage()
    {
        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        if (playerData != null && CardControl != null)
        {
            int maxHealth = playerData.playerMaxHealth;
            int currentHealth = playerData.playerHealth;
            int healthLost = maxHealth - currentHealth;
            int extraDamage = healthLost / 3;

            if (extraDamage > 0)
            {
                CardControl.AddEffectValue(EEffectType.Damage, extraDamage);
            }
        }
    }
}

