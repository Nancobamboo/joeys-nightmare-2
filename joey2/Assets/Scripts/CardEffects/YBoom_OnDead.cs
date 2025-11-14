// Scripts/CardEffects/Effects/YBoom_OnKill.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class YBoom_OnDead : YCardEffect
{
	public int baseExtra;

	int m_EnvIndex = -1;

	public YBoom_OnDead(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.Boom_OnDead;
	}

	public override float OnBeDying()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			m_EnvIndex = CardControl.EnvIndex;
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_feitian, EVFXLife.SelfLife);
			return maxDelayTime > 0f ? maxDelayTime : base.OnBeDying();
		}
		return base.OnBeDying();
	}

	public override float OnDead()
	{
		YActionSystem.Instance.DispatchAction(EActionId.BoomEnvCard, m_EnvIndex, baseExtra, true);
		return 0f;
	}



	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Damage)
		{
			return baseExtra;
		}
		return base.GetEffectValue(effectType);
	}
}

