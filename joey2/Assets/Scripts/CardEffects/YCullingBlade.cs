// Scripts/CardEffects/Effects/YCullingBlade.cs
// 斩杀之刃效果：当目标怪物当前HP小于最大HP的一半时，造成等于其剩余生命值的伤害
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YCullingBlade : YDefaultEffect
{
	public YCullingBlade()
	{
		Id = ECardEffectId.CullingBlade;
	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Damage)
		{
			// 获取当前攻击目标
			UICardSimpleControl targetCard = JoeyGameControl.Instance?.GetCurrentAttackTarget();
			if (targetCard != null && targetCard.CardData != null)
			{
				int currentHealth = targetCard.CardData.currentHealth;
				int maxHealth = targetCard.CardData.health;

				// 如果目标当前HP小于最大HP的一半，触发斩杀
				if (currentHealth < maxHealth / 2.0f)
				{
					// 计算需要的额外伤害，使得总伤害等于目标当前HP
					int weaponDamage = CardControl?.CardData?.currentAttack ?? 0;
					int extraDamage = Mathf.Max(0, currentHealth - weaponDamage);
					return extraDamage;
				}
			}
			return 0;
		}
		return base.GetEffectValue(effectType);
	}
}

public partial class UIGamePhaseControl
{
	/// <summary>
	/// 获取当前攻击目标的卡牌控制器
	/// </summary>
	public UICardSimpleControl GetCurrentAttackTarget()
	{
		if (m_CurrentAttackTargetEnvIndex < 0 || m_CurrentAttackTargetEnvIndex >= m_EnvPanels.Count)
		{
			return null;
		}
		return GetLastEnvCard(m_CurrentAttackTargetEnvIndex);
	}
}
