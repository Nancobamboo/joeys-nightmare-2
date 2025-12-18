using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YFreshBloodGuard_UseItem : YCardEffect
{
    public int baseExtra;

    public YFreshBloodGuard_UseItem(int baseExtra)
    {
        Id = ECardEffectId.FreshBloodGuard_UseItem;
        this.baseExtra = baseExtra;
    }

    public override float UseItem()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.FreshBloodGuardActivate, baseExtra);

            return 0.3f;
        }
        return base.UseItem();
    }
}

public partial class UIGamePhaseControl
{
    void FreshBloodGuardActivate(object[] paraArray)
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

        List<UICardSimpleControl> defenceCardList = GetBagCardList(ECardType.defence);
        if (defenceCardList != null)
        {
            for (int i = 0; i < defenceCardList.Count; i++)
            {
                UICardSimpleControl cardControl = defenceCardList[i];
                if (cardControl != null)
                {
                    cardControl.AddEffectValue(EEffectType.Defence, effectValue);
                }
            }
        }
    }
}

