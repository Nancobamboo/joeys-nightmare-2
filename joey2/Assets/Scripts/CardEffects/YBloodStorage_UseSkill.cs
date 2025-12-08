// Scripts/CardEffects/Effects/YBloodStorage_UseSkill.cs
// 血量存储效果：扣除当前HP的一半，在接下来5个行动回合平均恢复
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YBloodStorage_UseSkill : YCardEffect
{
	public YBloodStorage_UseSkill()
	{
		Id = ECardEffectId.BloodStorage_UseSkill;
	}

	public override float UseSkill()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

			// 获取玩家当前HP
			DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
			int currentHealth = playerData.playerHealth;

			// 计算扣除的血量（向下取整）
			int healthToDeduct = currentHealth / 2;

			if (healthToDeduct > 0)
			{
				// 计算每回合恢复量（向下取整）
				int healPerTurn = healthToDeduct / 5;

				// 只有当每回合恢复量大于0时才激活效果
				if (healPerTurn > 0)
				{
					// 激活血量存储效果：传入每回合恢复量和持续回合数
					YActionSystem.Instance.DispatchAction(EActionId.BloodStorageActivate, healPerTurn, 5);

					// 扣除血量并更新UI
					YActionSystem.Instance.DispatchAction(EActionId.BloodStorageDeduct, healthToDeduct);

					Debug.Log($"BloodStorage: Deducted {healthToDeduct} HP, will heal {healPerTurn} HP per turn for 5 turns");
				}
			}

			return 0.3f;
		}
		return base.UseSkill();
	}
}

public partial class UIGamePhaseControl
{
	// 血量存储效果状态
	private int m_BloodStorageHealPerTurn = 0;
	private int m_BloodStorageRemainingTurns = 0;
	private bool m_BloodStorageSkipNextHeal = false; // 跳过当回合恢复的标志

	void BloodStorageActivate(object[] paraArray)
	{
		int healPerTurn = paraArray.Length > 0 && paraArray[0] is int ? (int)paraArray[0] : 0;
		int turns = paraArray.Length > 1 && paraArray[1] is int ? (int)paraArray[1] : 5;

		m_BloodStorageHealPerTurn = healPerTurn;
		m_BloodStorageRemainingTurns = turns;
		m_BloodStorageSkipNextHeal = true; // 使用技能牌当回合跳过恢复

		Debug.Log($"BloodStorage activated: {healPerTurn} HP per turn for {turns} turns");
	}

	void BloodStorageDeduct(object[] paraArray)
	{
		int damage = paraArray.Length > 0 && paraArray[0] is int ? (int)paraArray[0] : 0;
		if (damage > 0)
		{
			// 确保扣血后至少保留1点HP
			int currentHealth = m_DataJoeyPlayer.playerHealth;
			int actualDamage = Mathf.Min(damage, currentHealth - 1);
			if (actualDamage > 0)
			{
				ApplyPlayerHealthChange(-actualDamage);
			}
		}
	}

	// 在玩家行动完成后触发血量恢复
	private void TryBloodStorageHeal()
	{
		if (m_BloodStorageRemainingTurns > 0 && m_BloodStorageHealPerTurn > 0)
		{
			// 使用技能牌当回合跳过恢复
			if (m_BloodStorageSkipNextHeal)
			{
				m_BloodStorageSkipNextHeal = false;
				Debug.Log("BloodStorage: Skipped heal on activation turn");
				return;
			}

			m_BloodStorageRemainingTurns--;
			
			// 延迟恢复，避免与其他恢复效果动画重叠
			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				ApplyPlayerHealthChange(m_BloodStorageHealPerTurn, true);
				Debug.Log($"BloodStorage heal: {m_BloodStorageHealPerTurn} HP, {m_BloodStorageRemainingTurns} turns remaining");
			}, 0.3f);

			if (m_BloodStorageRemainingTurns <= 0)
			{
				m_BloodStorageHealPerTurn = 0;
				Debug.Log("BloodStorage effect ended");
			}
		}
	}
}
