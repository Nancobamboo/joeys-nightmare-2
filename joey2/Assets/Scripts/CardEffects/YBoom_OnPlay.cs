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



	public override float OnRemoveCard()
	{
		YActionSystem.Instance.DispatchAction(EActionId.BoomEnvCard, -1, baseExtra);
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

