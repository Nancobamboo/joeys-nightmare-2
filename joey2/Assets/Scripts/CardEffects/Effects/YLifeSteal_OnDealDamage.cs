// Scripts/CardEffects/Effects/YLifeSteal_OnDealDamage.cs
using System.Collections;
using UnityEngine;

public class YLifeSteal_OnDealDamage : YCardEffect
{
	public int baseExtra;

	public YLifeSteal_OnDealDamage(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.LifeSteal_OnDealDamage;
	}

	public override void OnDealDamage()
	{
		base.OnDealDamage();
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

