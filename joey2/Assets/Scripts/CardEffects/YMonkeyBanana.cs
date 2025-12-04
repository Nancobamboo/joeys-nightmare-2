using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YMonkeyBanana : YCardEffect
{
	public YMonkeyBanana()
	{
		Id = ECardEffectId.MonkeyBanana;
	}

	public override float UseItem()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
		}

		YActionSystem.Instance.DispatchAction(EActionId.PermanentBoostAttack);

		return 0.3f;
	}
}

