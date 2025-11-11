// Scripts/CardEffects/Effects/YExtraDamage_OnDealDamage.cs
using System.Collections;
using UnityEngine;

public class YExtraDamage_OnDealDamage : YCardEffect
{
	public int baseExtra;

	public YExtraDamage_OnDealDamage(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.ExtraDamage_OnDealDamage;
	}

	public override void OnDealDamage()
	{
		base.OnDealDamage();
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

