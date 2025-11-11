// Scripts/CardEffects/Effects/YBounceToRandomEnemy_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBounceToRandomEnemy_OnDealDamage : YCardEffect
{
	public int bounceCount;

	public YBounceToRandomEnemy_OnDealDamage(int bounceCount)
	{
		this.bounceCount = Mathf.Max(0, bounceCount);
		Id = ECardEffectId.BounceToRandomEnemy_OnDealDamage;
	}

	public override void OnDealDamage()
	{
		base.OnDealDamage();
	}
}

