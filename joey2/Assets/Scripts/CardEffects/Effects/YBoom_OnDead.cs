// Scripts/CardEffects/Effects/YBoom_OnKill.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class YBoom_OnDead : YCardEffect
{
	public int baseExtra;

	public YBoom_OnDead(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.Boom_OnDead;
	}

	public override float OnDead()
	{
		if (!IsEffecting)
		{
			IsEffecting = true;
			if (CardControl != null && CardControl.gameObject != null)
			{
				var vfxNames = new List<EVFXName> { };
				CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_feitian, EVFXLife.SelfLife, 0f);
			}
			int envIndex = CardControl.EnvIndex;
			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				YActionSystem.Instance.DispatchAction(EActionId.BoomEnvCard, envIndex, baseExtra);
			}, .7f);
			return 0.5f;
		}
		return base.OnDead();
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

