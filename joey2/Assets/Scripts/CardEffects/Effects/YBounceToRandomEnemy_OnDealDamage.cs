// Scripts/CardEffects/Effects/YBounceToRandomEnemy_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class YBounceToRandomEnemy_OnDealDamage : YDefaultEffect
{
	public int bounceCount;

	public YBounceToRandomEnemy_OnDealDamage(int bounceCount)
	{
		this.bounceCount = Mathf.Max(0, bounceCount);
		Id = ECardEffectId.BounceToRandomEnemy_OnDealDamage;
	}

	public override float OnDealDamage()
	{
		base.OnDealDamage();
		if (CardControl != null && CardControl.CardData != null)
		{
			int damage = CardControl.CardData.currentAttack;
			YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, bounceCount);
		}
		return base.OnDealDamage();
	}
}

