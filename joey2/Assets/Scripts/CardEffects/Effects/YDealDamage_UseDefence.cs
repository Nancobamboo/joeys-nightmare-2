// Scripts/CardEffects/Effects/YDealDamage_UseDefence.cs
using System.Collections;
using UnityEngine;

public class YDealDamage_UseDefence : YCardEffect
{
	public int baseExtra;

	public YDealDamage_UseDefence(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.DealDamage_UseDefence;
	}

	public override float UseDefence()
	{
		return base.UseDefence();
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

