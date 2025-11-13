// Scripts/CardEffects/Effects/YElectric.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YElectric : YCardEffect
{
	public int baseExtra;

	public YElectric(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.Electric;
	}

	public override float UseSkill()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
			return maxDelayTime > 0f ? maxDelayTime : base.UseSkill();
		}
		return base.UseSkill();
	}

	public override void OnUseFinished()
	{

		var vfxNames = new List<EVFXName> { };
		CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife);

		YActionSystem.Instance.DispatchAction(EActionId.TakeAllEnemyDamage);

	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Damage)
		{
			return baseExtra;
		}
		return base.GetEffectValue(effectType);
	}
}

