// Scripts/CardEffects/Effects/YKingShield_OnAttack.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YKingShield_OnAttack : YDefaultEffect
{
	// 切换计数，用于追踪这张卡被切换的次数
	public int SwitchCount = 0;

	public YKingShield_OnAttack()
	{
		Id = ECardEffectId.KingShield_OnAttack;
	}

	public override float OnRemoveCard()
	{
		// 如果切换计数小于2，则切换到盾牌
		if (SwitchCount < 2)
		{
			YActionSystem.Instance.DispatchAction(EActionId.KingShieldSwitchToShield, CardControl, SwitchCount);
		}
		// 如果计数已经达到2，正常进入弃牌，不触发切换
		return 0f;
	}
}
