// Scripts/CardEffects/Effects/YHealPlayer_OnDefense.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YHealPlayer_OnDefense : YCardEffect
{
	public YHealPlayer_OnDefense()
	{
		Id = ECardEffectId.HealPlayer_OnDefense;
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
		Debug.Log($"heal player on defense");
		YActionSystem.Instance.DispatchAction(EActionId.HealPlayerOnDefense, CardControl);
		return 0f;
	}
}