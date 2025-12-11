using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YPermanentAttackBoostWithRandomDamage : YDefaultEffect
{
	public int deltaPara;

	public YPermanentAttackBoostWithRandomDamage(int deltaPara)
	{
		this.deltaPara = deltaPara;
		Id = ECardEffectId.PermanentAttackBoostWithRandomDamage;
	}

	public override float OnKill()
	{
		if (CardControl != null && CardControl.CardData != null)
		{
			Card cardData = CardControl.CardData;
			cardData.currentAttack += deltaPara;
			CardControl.RefreshCard();
		}
		return base.OnKill();
	}

	public override float OnBecomeTopOfPile()
	{
		if (CardControl != null && CardControl.CardData != null)
		{
			if (JoeyGameControl.Instance.HasEnemy())
			{
				CardControl.PlayVFX(null, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);

				int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
				int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraAttackCnt) ?? 0);
				JoeyGameControl.Instance.AddGlobalDelayCall(() =>
				{
					YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, attackTime);
				}, 0.3f);
			}
		}
		return base.OnBecomeTopOfPile();
	}

	public override float OnEnterBag()
	{
		if (CardControl != null && CardControl.CardData != null)
		{
			if (JoeyGameControl.Instance.HasEnemy())
			{
				CardControl.PlayVFX(null, ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);
				int damage = CardControl.CardData.currentAttack + (CardControl.CardEffect?.GetEffectValue(EEffectType.Damage) ?? 0);
				int attackTime = 1 + (CardControl.CardEffect?.GetEffectValue(EEffectType.ExtraAttackCnt) ?? 0);
				JoeyGameControl.Instance.AddGlobalDelayCall(() =>
				{
					YActionSystem.Instance.DispatchAction(EActionId.AttackRandomEnemy, damage, attackTime);
				}, 0.3f);
			}
		}
		return base.OnEnterBag();
	}
}

