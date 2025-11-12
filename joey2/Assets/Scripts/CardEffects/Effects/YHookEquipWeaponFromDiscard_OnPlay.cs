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
			CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife, 0.65f);
		}

		JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		{
			YActionSystem.Instance.DispatchAction(EActionId.AddCardFromDiscard);
		}, .2f);
		return 0.65f;
	}
}

