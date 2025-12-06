using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBone : YCardEffect
{
	public YBone()
	{
		Id = ECardEffectId.Bone;
	}

	public override float UseItem()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
		}

		string[] cardIds = new string[] { "3007", "3008", "3009" };
		int randomIndex = Random.Range(0, cardIds.Length);
		string selectedCardId = cardIds[randomIndex];

		Card newCard = DataSystem.Instance.CreateCard(selectedCardId);
		if (newCard != null)
		{
			YActionSystem.Instance.DispatchAction(EActionId.AddCardToBagFromSelect, newCard);
		}

		return 0.3f;
	}
}

