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
	
	/// <summary>
	/// Calculate preview execute damage when hovering over a monster
	/// </summary>
	public int GetPreviewExecuteDamage()
	{
		UICardSimpleControl targetCard = JoeyGameControl.Instance?.GetCurrentAttackTarget();
		if (targetCard != null && targetCard.CardData != null)
		{
			int currentHealth = targetCard.CardData.currentHealth;
			int maxHealth = targetCard.CardData.health;
			float halfHealth = maxHealth / 2.0f;
			
			if (currentHealth <= halfHealth)
			{
				int weaponDamage = CardControl?.CardData?.currentAttack ?? 0;
				int currentEffectDamage = GetEffectValue(EEffectType.Damage);
				int totalDamage = weaponDamage + currentEffectDamage;
				int extraDamage = Mathf.Max(0, currentHealth - totalDamage);
				return extraDamage;
			}
		}
		return 0;
	}

	public override float OnDealDamage()
	{
		UICardSimpleControl targetCard = JoeyGameControl.Instance?.GetCurrentAttackTarget();
		if (targetCard != null && targetCard.CardData != null)
		{
			int currentHealth = targetCard.CardData.currentHealth;
			int maxHealth = targetCard.CardData.health;
			float halfHealth = maxHealth / 2.0f;

			Debug.Log($"[CullingBlade] Target: {targetCard.CardData.cardName}, Current HP: {currentHealth}, Max HP: {maxHealth}, Half HP: {halfHealth}, Can Execute: {currentHealth <= halfHealth}");

			if (currentHealth <= halfHealth)
			{
				int weaponDamage = CardControl?.CardData?.currentAttack ?? 0;
				int currentEffectDamage = GetEffectValue(EEffectType.Damage);
				int totalDamage = weaponDamage + currentEffectDamage;
				int extraDamage = Mathf.Max(0, currentHealth - totalDamage);
				
				Debug.Log($"[CullingBlade] Execute triggered! Extra damage needed: {extraDamage}");
				if (extraDamage > 0)
				{
					CardControl.AddEffectValue(EEffectType.Damage, extraDamage);
					Debug.Log($"[CullingBlade] Added {extraDamage} damage to execute target");
				}
			}
		}
		return base.OnDealDamage();
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
