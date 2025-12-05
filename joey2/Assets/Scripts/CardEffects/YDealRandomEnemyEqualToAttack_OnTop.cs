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
			CardControl.PlayVFX(null, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);

			int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
			int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraAttackCnt) ?? 0);
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
			CardControl.PlayVFX(null, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);
			int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
			int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraAttackCnt) ?? 0);
			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, attackTime);
			}, 0.4f);
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

		//Debug.Log("AttackRandomEnemy: attackTime = " + attackTime);
		for (int i = 0; i < attackTime; i++)
		{
			int envIndex = FindRandomEnemy();
			if (envIndex == -1)
			{
				return;
			}

			UICardSimpleControl enemyCardControl = GetLastEnvCard(envIndex);
			if (enemyCardControl == null || enemyCardControl.gameObject == null || !enemyCardControl.gameObject.activeSelf)
			{
				return;
			}

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

