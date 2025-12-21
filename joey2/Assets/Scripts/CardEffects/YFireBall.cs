using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YFireBall : YCardEffect
{
	public int baseExtra;
	public YFireBall(int baseExtra)
	{
		Id = ECardEffectId.FireBall;
		this.baseExtra = baseExtra;
	}

	public override float UseSkill()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

			// 触发火球伤害效果
			JoeyGameControl.Instance.QueueAction(EActionId.FireBallDamage, baseExtra);

			return 0.3f;
		}
		return base.UseSkill();
	}
}

public partial class UIGamePhaseControl
{
	void FireBallDamage(object[] paraArray)
	{
		int damage = paraArray[0] is int ? (int)paraArray[0] : 0;
		FireBallDamageAsync(damage).Forget();
	}

	async UniTask FireBallDamageAsync(int damage)
	{
		if (damage <= 0)
		{
			return;
		}

		// 获取所有有怪物的环境位置
		List<int> enemyIndices = new List<int>();
		for (int i = 0; i < m_EnvPanels.Count; i++)
		{
			UICardSimpleControl lastCard = GetLastEnvCard(i);
			if (lastCard != null && lastCard.gameObject.activeSelf && 
			    lastCard.CardType == ECardType.monster && lastCard.CardData.currentHealth > 0)
			{
				enemyIndices.Add(i);
			}
		}

		if (enemyIndices.Count == 0)
		{
			return;
		}

		// 计算每个位置周围有多少个怪物，优先选择周围有怪物的位置
		List<int> priorityIndices = new List<int>();
		int maxAdjacentCount = 0;

		foreach (int envIndex in enemyIndices)
		{
			int adjacentCount = 0;
			int[] adjacentIndices = new int[] { envIndex - 1, envIndex + 1 };

			foreach (int adjIndex in adjacentIndices)
			{
				if (adjIndex < 0 || adjIndex >= m_EnvPanels.Count)
				{
					continue;
				}

				UICardSimpleControl adjacentCard = GetLastEnvCard(adjIndex);
				if (adjacentCard != null && adjacentCard.gameObject.activeSelf && 
				    adjacentCard.CardType == ECardType.monster && adjacentCard.CardData.currentHealth > 0)
				{
					adjacentCount++;
				}
			}

			if (adjacentCount > maxAdjacentCount)
			{
				maxAdjacentCount = adjacentCount;
				priorityIndices.Clear();
				priorityIndices.Add(envIndex);
			}
			else if (adjacentCount == maxAdjacentCount)
			{
				priorityIndices.Add(envIndex);
			}
		}

		// 从优先级最高的位置中随机选择一个
		int targetEnvIndex = priorityIndices[Random.Range(0, priorityIndices.Count)];

		Debug.Log($"FireBall: Target envIndex = {targetEnvIndex}, damage = {damage}");

		// 收集目标和周围的怪物
		List<(UICardSimpleControl card, int envIndex)> targets = new List<(UICardSimpleControl, int)>();

		// 添加主目标
		UICardSimpleControl targetCard = GetLastEnvCard(targetEnvIndex);
		if (targetCard != null && targetCard.gameObject.activeSelf && 
		    targetCard.CardType == ECardType.monster && targetCard.CardData.currentHealth > 0)
		{
			targets.Add((targetCard, targetEnvIndex));
		}

		// 添加周围的怪物
		int[] adjIndices = new int[] { targetEnvIndex - 1, targetEnvIndex + 1 };
		foreach (int adjIndex in adjIndices)
		{
			if (adjIndex < 0 || adjIndex >= m_EnvPanels.Count)
			{
				continue;
			}

			UICardSimpleControl adjacentCard = GetLastEnvCard(adjIndex);
			if (adjacentCard != null && adjacentCard.gameObject.activeSelf && 
			    adjacentCard.CardType == ECardType.monster && adjacentCard.CardData.currentHealth > 0)
			{
				Debug.Log($"FireBall: Adjacent target envIndex = {adjIndex}");
				targets.Add((adjacentCard, adjIndex));
			}
		}

		// 同时对所有目标造成伤害
		if (targets.Count > 0)
		{
			List<UniTask> damageTasks = new List<UniTask>();
			foreach (var target in targets)
			{
				CancellationToken token = GetOrCreateCardToken(target.card);
				damageTasks.Add(DealDamageToEnvCard(target.card, damage, target.envIndex, EEffectType.FireBall, token));
			}

			await UniTask.WhenAll(damageTasks);

			// 清理所有目标的 CancellationTokenSource
			foreach (var target in targets)
			{
				RemoveCardCts(target.card);
			}
		}
	}
}
