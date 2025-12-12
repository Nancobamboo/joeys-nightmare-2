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

	public override float OnDealDamage()
	{
		CardControl.AddEffectValue(EEffectType.Damage, baseExtra);
		return base.OnDealDamage();
	}
}

