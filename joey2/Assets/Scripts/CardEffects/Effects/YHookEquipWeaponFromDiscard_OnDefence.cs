// Scripts/CardEffects/Effects/YHookEquipWeaponFromDiscard_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YHookEquipWeaponFromDiscard_OnDefence : YCardEffect
{
	public YHookEquipWeaponFromDiscard_OnDefence()
	{
		Id = ECardEffectId.HookEquipWeaponFromDiscard_OnDefence;
	}

	public override float UseDefence()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
			CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife, 0.65f);
		}

		JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		{
			YActionSystem.Instance.DispatchAction(EActionId.AddCardFromDiscard);
		}, .2f);
		return 0.65f;
	}
}

