// Scripts/CardEffects/Effects/YHookEquipWeaponFromDiscard_OnPlay.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YHookEquipWeaponFromDiscard_OnPlay : YCardEffect
{
	public YHookEquipWeaponFromDiscard_OnPlay()
	{
		Id = ECardEffectId.HookEquipWeaponFromDiscard_OnPlay;
	}

	public override float UseItem()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife);
			return maxDelayTime > 0f ? maxDelayTime : base.UseItem();
		}
		return base.UseItem();
	}

	public override void OnUseFinished()
	{

		YActionSystem.Instance.DispatchAction(EActionId.AddCardFromDiscard);

	}
}

