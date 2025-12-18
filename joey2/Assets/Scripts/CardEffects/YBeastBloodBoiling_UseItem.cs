using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YBeastBloodBoiling_UseItem : YCardEffect
{
    public int baseExtra;

    public YBeastBloodBoiling_UseItem(int baseExtra)
    {
        Id = ECardEffectId.BeastBloodBoiling_UseItem;
        this.baseExtra = baseExtra;
    }

    public override float UseItem()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.BeastBloodBoilingActivate, baseExtra);

            return 0.3f;
        }
        return base.UseItem();
    }
}

public partial class UIGamePhaseControl
{
    void BeastBloodBoilingActivate(object[] paraArray)
    {
        int baseExtra = (int)paraArray[0];

        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        int currentHealth = playerData.playerHealth;
        int healthToDeduct = currentHealth / 2;

        if (healthToDeduct > 0)
        {
            ApplyPlayerHealthChange(-healthToDeduct);
        }

        int effectValue = Mathf.Max(baseExtra, healthToDeduct);

        List<UICardSimpleControl> attackCardList = GetBagCardList(ECardType.attack);
        if (attackCardList != null)
        {
            for (int i = 0; i < attackCardList.Count; i++)
            {
                UICardSimpleControl cardControl = attackCardList[i];
                if (cardControl != null)
                {
                    cardControl.AddEffectValue(EEffectType.Damage, effectValue);
                }
            }
        }
    }
}

