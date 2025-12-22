using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YPermanentDefenceBoost : YDefaultEffect
{
	public int deltaPara;

	public YPermanentDefenceBoost(int deltaPara)
	{
		this.deltaPara = deltaPara;
		Id = ECardEffectId.PermanentDefenceBoost;
	}

	public override float OnBecomeTopOfPile()
	{
		if (CardControl != null && CardControl.CardData != null)
		{
			Card cardData = CardControl.CardData;
			cardData.currentDefence += deltaPara;
			CardControl.RefreshCard();
		}
		return base.OnBecomeTopOfPile();
	}

	public override float OnEnterBag()
	{
		// OnBecomeTopOfPile will handle defence boost
		if (CardControl != null && CardControl.CardData != null)
		{
			Debug.Log($"[PermanentDefenceBoost] OnEnterBag - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}, current defence: {CardControl.CardData.currentDefence}");
		}
		else
		{
			Debug.LogWarning($"[PermanentDefenceBoost] OnEnterBag - CardControl or CardData is null!");
		}
		return base.OnEnterBag();
	}
}

