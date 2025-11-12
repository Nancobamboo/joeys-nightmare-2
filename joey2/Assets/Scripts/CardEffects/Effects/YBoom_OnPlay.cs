// Scripts/CardEffects/Effects/YBoom_OnPlay.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YBoom_OnPlay : YCardEffect
{
	public int baseExtra;

	public YBoom_OnPlay(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.Boom_OnPlay;
	}

	public override void UseItem()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_boom };
			CardControl.PlayVFX(vfxNames, ECardAnimName.None, EVFXLife.SelfLife, 0.65f);
			SFX.PlayAudio("Audio/SFX/Battle/boom", 1.0f, 0f);
		}

		YActionSystem.Instance.DispatchAction(EActionId.BoomEnvCard, -1, baseExtra, CardControl);
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

