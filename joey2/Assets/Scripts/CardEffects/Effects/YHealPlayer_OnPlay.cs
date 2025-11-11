// Scripts/CardEffects/Effects/YHealPlayer_OnPlay.cs
using System.Collections;
using UnityEngine;

public class YHealPlayer_OnPlay : YCardEffect
{
	public int healAmount;

	public YHealPlayer_OnPlay(int healAmount)
	{
		this.healAmount = Mathf.Max(0, healAmount);
		Id = ECardEffectId.HealPlayer_OnPlay;
	}

	public override void OnPlay()
	{
		base.OnPlay();
	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Heal)
		{
			return healAmount;
		}
		return base.GetEffectValue(effectType);
	}
}

