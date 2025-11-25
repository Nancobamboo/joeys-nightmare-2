// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YAddKnifeToEnv_UseSkill : YCardEffect
{
	public YAddKnifeToEnv_UseSkill()
	{
		Id = ECardEffectId.AddKnifeToEnv_UseSkill;
	}
	public override float UseSkill()
	{

        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
            return 0.3f;
        }
		return base.UseSkill();
	}

	public override float OnRemoveCard()
	{
		YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "1004");
		YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "1004");
		return 0f;
	}

}
