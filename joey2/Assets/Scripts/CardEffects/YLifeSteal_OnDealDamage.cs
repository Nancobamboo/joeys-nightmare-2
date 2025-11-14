// Scripts/CardEffects/Effects/YLifeSteal_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YLifeSteal_OnDealDamage : YCardEffect
{
	public int baseExtra;

	public YLifeSteal_OnDealDamage(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.LifeSteal_OnDealDamage;
	}

	public override float OnDealDamage()
	{
		if (baseExtra > 0 && CardControl != null)
		{
			YActionSystem.Instance.DispatchAction(EActionId.AppHp, baseExtra);

			// if (CardControl.gameObject != null)
			// {
			// 	var vfxNames = new List<EVFXName> { };
			// 	CardControl.PlayVFX(vfxNames, ECardAnimName.Idle, EVFXLife.CardLife);
			// }
		}
		return base.OnDealDamage();
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

