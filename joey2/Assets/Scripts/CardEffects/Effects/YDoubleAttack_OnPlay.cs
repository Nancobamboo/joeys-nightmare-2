// Scripts/CardEffects/Effects/YDoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class YDoubleAttack_OnPlay : YDefaultEffect
{
	public YDoubleAttack_OnPlay()
	{
		Id = ECardEffectId.DoubleAttack_OnPlay;
	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.ExtraTime)
		{
			return 1;
		}
		return base.GetEffectValue(effectType);
	}
}

