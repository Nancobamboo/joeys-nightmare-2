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
		YActionSystem.Instance.DispatchAction(EActionId.AddCardFromDiscard);

		return base.UseItem();
	}

}

