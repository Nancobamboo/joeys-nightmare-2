// Scripts/CardEffects/Effects/YDealDamage_UseDefence.cs
using System.Collections;
using System.Collections.Generic;
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
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.CardLife);
			return maxDelayTime > 0f ? maxDelayTime : base.UseDefence();
		}
		return base.UseDefence();
	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.ReflectDamage)
		{
			return baseExtra;
		}
		return base.GetEffectValue(effectType);
	}
}

