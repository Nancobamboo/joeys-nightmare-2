using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YVengeanceShield : YCardEffect
{
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
            UpdateVengeanceShieldDefence();
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.UpdateByHpChange)
        {
            CardControl.ClearEffectVlaue();
            UpdateVengeanceShieldDefence();
        }
        return value;
    }

    private void UpdateVengeanceShieldDefence()
    {
        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        if (playerData != null && CardControl != null)
        {
            int maxHealth = playerData.playerMaxHealth;
            int currentHealth = playerData.playerHealth;
            int healthLost = maxHealth - currentHealth;
            int extraDefence = healthLost / 3;

            if (extraDefence > 0)
            {
                CardControl.AddEffectValue(EEffectType.Defence, extraDefence);
            }
        }
    }
}

