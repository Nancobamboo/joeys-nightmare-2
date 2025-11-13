// Scripts/CardEffects/Effects/YHookEquipWeaponFromDiscard_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YHookEquipWeaponFromDiscard_OnDefence : YCardEffect
{
	public YHookEquipWeaponFromDiscard_OnDefence()
	{
		Id = ECardEffectId.HookEquipWeaponFromDiscard_OnDefence;
	}

	public override float UseDefence()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
			SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
			return maxDelayTime > 0f ? maxDelayTime : base.UseDefence();
		}
		return base.UseDefence();
	}

	public override float OnUseFinished()
	{
		YActionSystem.Instance.DispatchAction(EActionId.AddCardFromDiscard, ECardType.attack);
		return 0f;
	}
}

