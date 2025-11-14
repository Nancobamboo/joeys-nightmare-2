// Scripts/CardEffects/Effects/YDoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class YDoubleAttack_OnPlay : YCardEffect
{
	public YDoubleAttack_OnPlay()
	{
		Id = ECardEffectId.DoubleAttack_OnPlay;
		AddEffectValue(EEffectType.ExtraAttackCnt, 1);
	}

	public override float OnRemoveCard()
	{
		int extraAttackCnt = GetEffectValue(EEffectType.ExtraAttackCnt);
		YActionSystem.Instance.DispatchAction(EActionId.AddEffectValueToBagCard, ECardType.attack, EEffectType.ExtraAttackCnt, extraAttackCnt);
		return 0f;
	}
}

