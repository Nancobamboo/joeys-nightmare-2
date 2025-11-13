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
		if (CardControl != null && CardControl.CardData != null)
		{
			int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
			int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraTime) ?? 0);
			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, attackTime);
			}, 0.4f);
		}
		return base.OnBecomeTopOfPile();
	}

	public override float OnEnterBag()
	{
		if (CardControl != null && CardControl.CardData != null)
		{
			int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
			int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraTime) ?? 0);
			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, attackTime);
			}, 0.4f);
		}
		return base.OnEnterBag();
	}
}

