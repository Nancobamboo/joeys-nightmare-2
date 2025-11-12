// Scripts/CardEffects/Effects/YBoom_OnKill.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_boom };
			CardControl.PlayVFX(vfxNames, ECardAnimName.None, EVFXLife.SelfLife, 0.65f);
			SFX.PlayAudio("Audio/SFX/Battle/boom", 1.0f, 0f);
		}

		int envIndex = CardControl.EnvIndex;
		YActionSystem.Instance.DispatchAction(EActionId.BoomEnvCard, envIndex, baseExtra);
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

