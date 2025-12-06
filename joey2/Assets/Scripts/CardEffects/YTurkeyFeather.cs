using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YTurkeyFeather : YCardEffect
{
	public YTurkeyFeather()
	{
		Id = ECardEffectId.TurkeyFeather;
	}

	public override float UseItem()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
		}

		YActionSystem.Instance.DispatchAction(EActionId.PermanentBoostDefence);

		return 0.3f;
	}
}

