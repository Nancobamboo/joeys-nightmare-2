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

	public override void SetData(UICardSimpleControl cardControl)
	{
		base.SetData(cardControl);
		if (CardControl != null)
		{
			AddEffectValue(EEffectType.ReflectDamage, baseExtra);
		}
	}

	public override float UseDefence(bool isOverflow = false)
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_Fanjia };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.CardLife);
			SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
			return maxDelayTime > 0f ? maxDelayTime : base.UseDefence(isOverflow);
		}
		return base.UseDefence(isOverflow);
	}
}

