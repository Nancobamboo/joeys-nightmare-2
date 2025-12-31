// Scripts/CardEffects/Effects/YDealRandomEnemyEqualToAttack_OnTop.cs
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
					YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemyAndClearEffect, damage, attackTime, CardControl);
				}, 0.3f);
			}
		}
		return base.OnBecomeTopOfPile();
	}

	public override float OnEnterBag()
	{
		// OnBecomeTopOfPile will handle the auto-attack
		if (CardControl != null && CardControl.CardData != null)
		{
			Debug.Log($"[Shuriken] OnEnterBag - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}");
		}
		else
		{
			Debug.LogWarning($"[Shuriken] OnEnterBag - CardControl or CardData is null!");
		}
		return base.OnEnterBag();
	}
}

public partial class UIGamePhaseControl
{
	public async UniTask AttackRandomEnemy(int damage, int attackTime)
	{
		if (DataSystem.Instance.HasRelic(ERelicType.ShurikenMastery))
		{
			damage += 2;
		}

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
			else if (enemyCardControl.CardEffect?.GetEffectValue(EEffectType.QuickAttack) > 0)
			{
				int enemyAttack = enemyCardControl.CardData.currentAttack;
				CancellationToken enemyToken = GetOrCreateCardToken(enemyCardControl);
				await TakePlayerDamageAsync(enemyAttack, enemyCardControl, envIndex, enemyToken, null);
			}
		}
	}
}

