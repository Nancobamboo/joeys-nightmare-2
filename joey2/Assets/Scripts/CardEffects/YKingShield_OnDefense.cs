// Scripts/CardEffects/Effects/YKingShield_OnDefense.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YKingShield_OnDefense : YCardEffect
{
	// 切换计数，用于追踪这张卡被切换的次数
	public int SwitchCount = 0;

	public YKingShield_OnDefense()
	{
		Id = ECardEffectId.KingShield_OnDefense;
	}

	public override float UseDefence(bool isOverflow = false)
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
			SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
			return maxDelayTime > 0f ? maxDelayTime : base.UseDefence(isOverflow);
		}
		return base.UseDefence(isOverflow);
	}

	public override float OnRemoveCard()
	{
		// 如果切换计数小于2，则切换到剑
		if (SwitchCount < 2)
		{
			YActionSystem.Instance.DispatchAction(EActionId.KingShieldSwitchToSword, CardControl, SwitchCount);
		}
		// 如果计数已经达到2，正常进入弃牌，不触发切换
		return 0f;
	}
}
