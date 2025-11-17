// Scripts/CardEffects/Effects/YLifeSteal_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YLifeSteal_OnDealDamage : YCardEffect
{
	public int baseExtra;

	public YLifeSteal_OnDealDamage(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.LifeSteal_OnDealDamage;
	}

	public override float OnDealDamage()
	{
		if (baseExtra > 0 && CardControl != null)
		{
			YActionSystem.Instance.DispatchAction(EActionId.AppHp, baseExtra);

			// if (CardControl.gameObject != null)
			// {
			// 	var vfxNames = new List<EVFXName> { };
			// 	CardControl.PlayVFX(vfxNames, ECardAnimName.Idle, EVFXLife.CardLife);
			// }
		}
		return base.OnDealDamage();
	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Heal)
		{
			return baseExtra;
		}
		return base.GetEffectValue(effectType);
	}
}

public partial class UIGamePhaseControl
{
	public void AppHp(int delta)
	{
		m_DataJoeyPlayer.lastPlayerHealth = m_DataJoeyPlayer.playerHealth;
		int oldHealth = m_DataJoeyPlayer.playerHealth;
		m_DataJoeyPlayer.playerHealth += delta;
		if (m_DataJoeyPlayer.playerHealth > m_DataJoeyPlayer.playerMaxHealth)
		{
			m_DataJoeyPlayer.playerHealth = m_DataJoeyPlayer.playerMaxHealth;
		}
		OnHPChanged(m_DataJoeyPlayer.playerHealth);

		if (delta > 0)
		{
			int actualHeal = m_DataJoeyPlayer.playerHealth - oldHealth;
			if (actualHeal > 0)
			{
				ShowDamageText(actualHeal, m_View.JoeyImage.transform, new Vector3(100f, 190f, 0), false);
			}
		}
	}
}

