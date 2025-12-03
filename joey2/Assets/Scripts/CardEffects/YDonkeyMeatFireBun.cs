using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YDonkeyMeatFireBun : YCardEffect
{
    public YDonkeyMeatFireBun()
    {
        Id = ECardEffectId.DonkeyMeatFireBun;
    }

    public override float UseSkill()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
        }

        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        int oldMaxHealth = playerData.playerMaxHealth;
        playerData.playerMaxHealth += 1;

        YActionSystem.Instance.DispatchAction(EActionId.AppHp, 1);


        return 0.3f;
    }
}

