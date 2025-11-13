// Scripts/CardEffects/Effects/YHealPlayer_OnPlay.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife);
			return maxDelayTime > 0f ? maxDelayTime : base.UseItem();
		}
		return base.UseItem();
	}

	public override void OnUseFinished()
	{
		YActionSystem.Instance.DispatchAction(EActionId.AppHp, healAmount);
	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Heal)
		{
			return healAmount;
		}
		return base.GetEffectValue(effectType);
	}
}

