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

	public override float UseDefence(bool isOverflow = false)
	{
		if (CardControl != null && CardControl.CardData != null)
		{
			Card cardData = CardControl.CardData;
			cardData.defence += deltaPara;
			cardData.currentDefence += deltaPara;
			CardControl.RefreshCard();
		}
		return base.UseDefence(isOverflow);
	}
}

