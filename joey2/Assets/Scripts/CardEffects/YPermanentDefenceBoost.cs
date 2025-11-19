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
			string cardId = CardControl.CardData.id;
			JoeyGameControl.Instance.AddEffectDefenceCard(cardId, deltaPara);
		}
		return base.UseDefence(isOverflow);
	}
}

