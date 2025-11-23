// Scripts/CardEffects/Effects/YHookEquipWeaponFromDiscard_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class HookDefenceFromDiscard_OnRemoveCard : YCardEffect
{
	public HookDefenceFromDiscard_OnRemoveCard()
	{
		Id = ECardEffectId.HookDefenceFromDiscard_OnRemoveCard;
	}





	public override float OnRemoveCard()
	{
		YActionSystem.Instance.DispatchAction(EActionId.AddCardFromDiscard, ECardType.defence);
		return 0f;
	}
}
