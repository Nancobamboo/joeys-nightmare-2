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

	public override void OnDead()
	{
		if (!IsEffecting)
		{
			IsEffecting = true;
			int envIndex = CardControl.EnvIndex;
			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				YActionSystem.Instance.DispatchAction(EActionId.BoomEnvCard, envIndex, baseExtra);
			}, 1f);
		}
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

