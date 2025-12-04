using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBone : YCardEffect
{
	public YBone()
	{
		Id = ECardEffectId.Bone;
	}

	public override float UseItem()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
		}

		if (ControlUtil.IsRandomSucceed(33))
		{
			YActionSystem.Instance.DispatchAction(EActionId.PermanentBoostAttack);
		}
		else if (ControlUtil.IsRandomSucceed(50))
		{
			YActionSystem.Instance.DispatchAction(EActionId.PermanentBoostDefence);
		}
		else
		{
			DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
			int oldMaxHealth = playerData.playerMaxHealth;
			playerData.playerMaxHealth += 1;

			YActionSystem.Instance.DispatchAction(EActionId.AppHp, 1);
		}
		return 0.3f;
	}
}

