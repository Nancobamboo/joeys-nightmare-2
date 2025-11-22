// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YThrowWeaponToStack_OnDefence : YCardEffect
{
	public YThrowWeaponToStack_OnDefence()
	{
		Id = ECardEffectId.ThrowWeaponToStack_OnDefence;
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
		YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponToEnv, CardControl);
		return 0f;
	}
}

public partial class UIGamePhaseControl
{
	public void ThrowWeaponToEnv(UICardSimpleControl cardControl)
	{
		UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
		if (weaponCard != null)
		{
			YActionSystem.Instance.DispatchAction(EActionId.AddEnvCardFromBag, weaponCard);
		}
	}
}