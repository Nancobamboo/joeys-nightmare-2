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
				YActionSystem.Instance.DispatchAction(EActionId.BloodStorageDeduct, healthToDeduct);
			}

			YActionSystem.Instance.DispatchAction(EActionId.BloodStorageActivate, maxHealth, 5);

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

	private void ResetBloodStorageState()
	{
		m_BloodStorageMaxHealth = 0;
		m_BloodStorageRemainingTurns = 0;
		m_BloodStorageSkipNextHeal = false;
	}

	void BloodStorageActivate(object[] paraArray)
	{
		int maxHealth = paraArray.Length > 0 && paraArray[0] is int ? (int)paraArray[0] : 0;
		int turns = paraArray.Length > 1 && paraArray[1] is int ? (int)paraArray[1] : 5;

		m_BloodStorageMaxHealth = maxHealth;
		m_BloodStorageRemainingTurns = turns;
		m_BloodStorageSkipNextHeal = true;
	}

	void BloodStorageDeduct(object[] paraArray)
	{
		int damage = paraArray.Length > 0 && paraArray[0] is int ? (int)paraArray[0] : 0;
		if (damage > 0)
		{
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
				return;
			}

			m_BloodStorageRemainingTurns--;

			int healAmount = m_BloodStorageMaxHealth / 10;

			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				ApplyPlayerHealthChange(healAmount, true);
			}, 0.3f);

			if (m_BloodStorageRemainingTurns <= 0)
			{
				m_BloodStorageMaxHealth = 0;
			}
		}
	}
}
