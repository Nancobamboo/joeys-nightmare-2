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
			CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife, 0.65f);
		}

		JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		{
			YActionSystem.Instance.DispatchAction(EActionId.TakeAllEnemyDamage);
		}, 0.2f);

		return 0.65f;
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

