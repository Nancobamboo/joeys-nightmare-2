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
		DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
		bool hasMastery = DataSystem.Instance.HasRelic(ERelicType.ShurikenMastery);
		bool hasGrowth = DataSystem.Instance.HasRelic(ERelicType.ShurikenGrowth);
		int hitDamage = damage;
		if (hasMastery) hitDamage += 2;
		if (playerData != null) hitDamage += playerData.shurikenAutoAttackBonus;

		if (hitDamage <= 0)
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
			bool isKilled = await DealDamageToEnvCard(enemyCardControl, hitDamage, envIndex, EEffectType.Damage, default);

			// 手里剑成长：每次触发一次手里剑自动攻击，有10%概率永久+1自动攻击伤害
			if (hasGrowth && playerData != null && ControlUtil.IsRandomSucceed(10))
			{
				playerData.shurikenAutoAttackBonus += 1;
			}

			if (isKilled)
			{
				break;
			}
			else if (enemyCardControl.CardEffect?.GetEffectValue(EEffectType.QuickAttack) > 0)
			{
				int enemyAttack = enemyCardControl.CardData.currentAttack;
				await TakePlayerDamageAsync(enemyAttack, enemyCardControl, envIndex, default, null);
			}
		}
	}
}

