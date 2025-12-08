// Scripts/CardEffects/Effects/YDealRandomEnemyEqualToAttack_OnTop.cs
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
			if (JoeyGameControl.Instance.HasEnemy())
			{
				CardControl.PlayVFX(null, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);

				JoeyGameControl.Instance.AddGlobalDelayCall(() =>
				{
					int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
					int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraAttackCnt) ?? 0);
					YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, attackTime);
				}, 0.3f);
			}
		}
		return base.OnBecomeTopOfPile();
	}

	public override float OnEnterBag()
	{
		if (CardControl != null && CardControl.CardData != null)
		{
			if (JoeyGameControl.Instance.HasEnemy())
			{
				CardControl.PlayVFX(null, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);

				JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		   		{
					   int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
					   int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraAttackCnt) ?? 0);
					   YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, attackTime);
		   		}, 0.3f);

			}
		}
		return base.OnEnterBag();
	}
}

public partial class UIGamePhaseControl
{
	public async UniTask AttackRandomEnemy(int damage, int attackTime)
	{

		if (damage <= 0)
		{
			return;
		}

		int envIndex = FindRandomEnemy();
		if (envIndex == -1)
		{
			return;
		}

		UICardSimpleControl enemyCardControl = GetLastEnvCard(envIndex);
		if (enemyCardControl == null)
		{
			return;
		}

		Debug.Log("AttackRandomEnemy: attackTime = " + attackTime);
		for (int i = 0; i < attackTime; i++)
		{
			CancellationToken token = GetOrCreateCardToken(enemyCardControl);
			bool isKilled = await DealDamageToEnvCard(enemyCardControl, damage, envIndex, EEffectType.Damage, token);


			RemoveCardCts(enemyCardControl);

			if (isKilled)
			{
				break;
			}

		}

	}
}

