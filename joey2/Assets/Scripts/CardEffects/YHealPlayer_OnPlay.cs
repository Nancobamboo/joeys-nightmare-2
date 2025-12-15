// Scripts/CardEffects/Effects/YHealPlayer_OnPlay.cs
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YHealPlayer_OnPlay : YCardEffect
{
	public int healAmount;

	public YHealPlayer_OnPlay(int healAmount)
	{
		this.healAmount = Mathf.Max(0, healAmount);
		Id = ECardEffectId.HealPlayer_OnPlay;
	}

	public override float UseItem()
	{
		return base.UseItem();
	}

	public override float OnRemoveCard()
	{
		if (DataSystem.Instance.HasRelic(ERelicType.HealEnhance))
		{
			healAmount += 5;
		}
		YActionSystem.Instance.DispatchAction(EActionId.AddHp, healAmount);
		return 0f;
	}
}

