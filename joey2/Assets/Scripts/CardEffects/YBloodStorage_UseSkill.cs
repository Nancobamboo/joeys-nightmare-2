// Scripts/CardEffects/Effects/YBloodStorage_UseSkill.cs
// 血量存储效果：扣除当前HP的一半，在接下来5个行动回合平均恢复
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
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

			DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
			int currentHealth = playerData.playerHealth;
			int maxHealth = playerData.playerMaxHealth;

			int healthToDeduct = currentHealth / 2;

			if (healthToDeduct > 0)
			{
				YActionSystem.Instance.DispatchAction(EActionId.BloodStorageActivate, maxHealth, 5);
				YActionSystem.Instance.DispatchAction(EActionId.BloodStorageDeduct, healthToDeduct);

				Debug.Log($"BloodStorage: Deducted {healthToDeduct} HP, will heal 10% max HP ({maxHealth / 10} HP) per turn for 5 turns");
			}

			return 0.3f;
		}
		return base.UseSkill();
	}
}

public partial class UIGamePhaseControl
{
	private int m_BloodStorageMaxHealth = 0;
	private int m_BloodStorageRemainingTurns = 0;
	private bool m_BloodStorageSkipNextHeal = false;

	void BloodStorageActivate(object[] paraArray)
	{
		int maxHealth = paraArray.Length > 0 && paraArray[0] is int ? (int)paraArray[0] : 0;
		int turns = paraArray.Length > 1 && paraArray[1] is int ? (int)paraArray[1] : 5;

		m_BloodStorageMaxHealth = maxHealth;
		m_BloodStorageRemainingTurns = turns;
		m_BloodStorageSkipNextHeal = true;

		Debug.Log($"BloodStorage activated: will heal 10% max HP ({maxHealth / 10} HP) per turn for {turns} turns");
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

	private void TryBloodStorageHeal()
	{
		if (m_BloodStorageRemainingTurns > 0 && m_BloodStorageMaxHealth > 0)
		{
			if (m_BloodStorageSkipNextHeal)
			{
				m_BloodStorageSkipNextHeal = false;
				Debug.Log("BloodStorage: Skipped heal on activation turn");
				return;
			}

			m_BloodStorageRemainingTurns--;

			int healAmount = m_BloodStorageMaxHealth / 10;

			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				ApplyPlayerHealthChange(healAmount, true);
				Debug.Log($"BloodStorage heal: {healAmount} HP (10% of {m_BloodStorageMaxHealth}), {m_BloodStorageRemainingTurns} turns remaining");
			}, 0.3f);

			if (m_BloodStorageRemainingTurns <= 0)
			{
				m_BloodStorageMaxHealth = 0;
				Debug.Log("BloodStorage effect ended");
			}
		}
	}
}
