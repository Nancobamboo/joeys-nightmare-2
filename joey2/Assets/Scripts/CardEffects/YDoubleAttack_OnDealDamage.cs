// Scripts/CardEffects/Effects/YDoubleAttack_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YDoubleAttack_OnDealDamage : YCardEffect
{
	public YDoubleAttack_OnDealDamage()
	{
		Id = ECardEffectId.DoubleAttack_OnDealDamage;
	}

	public override void SetData(UICardSimpleControl cardControl)
	{
		base.SetData(cardControl);
		AddEffectValue(EEffectType.ExtraAttackCnt, 1);
	}


}

