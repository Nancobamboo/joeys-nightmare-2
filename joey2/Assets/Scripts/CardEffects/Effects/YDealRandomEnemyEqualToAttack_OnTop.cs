// Scripts/CardEffects/Effects/YDealRandomEnemyEqualToAttack_OnTop.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YDealRandomEnemyEqualToAttack_OnTop : YDefaultEffect
{
	public YDealRandomEnemyEqualToAttack_OnTop()
	{
		Id = ECardEffectId.DealRandomEnemyEqualToAttack_OnTop;
	}

	public override float OnBecomeTopOfPile()
	{
		JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		{
			YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, CardControl);
		}, 0.4f);
		return base.OnBecomeTopOfPile();
	}

	public override float OnEnterBag()
	{
		JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		{
			YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, CardControl);
		}, 0.4f);
		return base.OnEnterBag();
	}
}

